using System.Text;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;
using Daibitx.HybridCache.Core.Implementations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Daibitx.HybridCache.Redis.Implementations;

/// <summary>
/// Redis 布隆过滤器实现
/// </summary>
public class RedisBloomFilter : IBloomFilter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IOptions<HybridCacheOptions> _options;
    private readonly ILogger<RedisBloomFilter>? _logger;
    private readonly BloomFilterCore _core;
    private readonly string _bitmapKey;

    public string Name => "RedisBloomFilter";

    public RedisBloomFilter(
        IConnectionMultiplexer redis,
        IOptions<HybridCacheOptions> options,
        ILogger<RedisBloomFilter>? logger = null)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        var bloomOptions = options.Value.BloomFilter;
        _core = new BloomFilterCore(bloomOptions.ExpectedElements, bloomOptions.FalsePositiveRate);
        _bitmapKey = $"{bloomOptions.KeyPrefix}:bitmap";
    }

    public async Task AddAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var positions = _core.GetHashPositions(key).ToArray();
            var db = _redis.GetDatabase();

            // 使用 Lua 脚本原子化设置多个位
            var script = @"
                local bitmapKey = KEYS[1]
                local positions = ARGV
                local count = 0
                
                for i = 1, #positions do
                    local pos = tonumber(positions[i])
                    if redis.call('SETBIT', bitmapKey, pos, 1) == 0 then
                        count = count + 1
                    end
                end
                
                return count
            ";

            var args = positions.Select(p => (RedisValue)p).ToArray();
            await db.ScriptEvaluateAsync(script, new RedisKey[] { _bitmapKey }, args);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error adding key to bloom filter: {Key}", key);
            throw;
        }
    }

    public async Task AddAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var db = _redis.GetDatabase();
            var allPositions = new List<RedisValue>();

            // 收集所有位置
            foreach (var key in keys)
            {
                var positions = _core.GetHashPositions(key);
                allPositions.AddRange(positions.Select(p => (RedisValue)p));
            }

            if (allPositions.Count == 0)
                return;

            // 使用 Lua 脚本批量设置
            var script = @"
                local bitmapKey = KEYS[1]
                local positions = ARGV
                local hashCount = tonumber(ARGV[#ARGV])
                local count = 0
                
                for i = 1, #positions - 1, hashCount do
                    for j = 0, hashCount - 1 do
                        local pos = tonumber(positions[i + j])
                        redis.call('SETBIT', bitmapKey, pos, 1)
                    end
                end
                
                return 1
            ";

            allPositions.Add(_core.GetHashPositions("dummy").Count()); // 添加哈希函数数量
            await db.ScriptEvaluateAsync(script, new RedisKey[] { _bitmapKey }, allPositions.ToArray());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error adding keys to bloom filter");
            throw;
        }
    }

    public async Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var positions = _core.GetHashPositions(key).ToArray();
            var db = _redis.GetDatabase();

            // 使用 Lua 脚本原子化检查多个位
            var script = @"
                local bitmapKey = KEYS[1]
                local positions = ARGV
                
                for i = 1, #positions do
                    local pos = tonumber(positions[i])
                    if redis.call('GETBIT', bitmapKey, pos) == 0 then
                        return 0
                    end
                end
                
                return 1
            ";

            var args = positions.Select(p => (RedisValue)p).ToArray();
            var result = await db.ScriptEvaluateAsync(script, new RedisKey[] { _bitmapKey }, args);
            
            return (int)result == 1;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking bloom filter for key: {Key}", key);
            // 发生错误时，返回 true 以避免误判为缓存穿透
            return true;
        }
    }

    public async Task<IEnumerable<bool>> ContainsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var db = _redis.GetDatabase();
            var results = new List<bool>();

            // 批量检查，每个 key 单独检查
            foreach (var key in keys)
            {
                var contains = await ContainsAsync(key, cancellationToken);
                results.Add(contains);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking bloom filter for keys");
            // 发生错误时，返回全部 true 以避免误判为缓存穿透
            return keys.Select(_ => true);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(_bitmapKey);
            _logger?.LogInformation("Bloom filter cleared");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error clearing bloom filter");
            throw;
        }
    }

    public async Task<BloomFilterStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var db = _redis.GetDatabase();
            
            // 获取 bitmap 的位数
            var bitmapLength = await db.StringLengthAsync(_bitmapKey);
            var setBitsCount = 0L;

            // 计算设置的位数（使用 BITCOUNT）
            if (await db.KeyExistsAsync(_bitmapKey))
            {
                var countResult = await db.ExecuteAsync("BITCOUNT", _bitmapKey);
                setBitsCount = (long)countResult;
            }

            return _core.GetStats(setBitsCount);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting bloom filter stats");
            throw;
        }
    }
}