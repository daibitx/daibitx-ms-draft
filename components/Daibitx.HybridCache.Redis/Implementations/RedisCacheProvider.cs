using Daibitx.HybridCache.Abstraction.Enums;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;
using Daibitx.HybridCache.Core.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace Daibitx.HybridCache.Redis.Implementations;

/// <summary>
/// Redis缓存提供者实现
/// </summary>
public class RedisCacheProvider : ICacheProvider
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IOptions<HybridCacheOptions> _options;
    private readonly ICacheStatistics? _statistics;
    private readonly ILogger<RedisCacheProvider>? _logger;

    public string Name => "RedisCache";

    public RedisCacheProvider(
        IConnectionMultiplexer redis,
        IOptions<HybridCacheOptions> options,
        ICacheStatistics? statistics = null,
        ILogger<RedisCacheProvider>? logger = null)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _statistics = statistics;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            
            if (value.IsNull)
            {
                _statistics?.RecordMiss();
                return null;
            }
            
            _statistics?.RecordHit();
            
            // 处理空值缓存
            if (value == "__NULL__")
            {
                return null;
            }
            
            // 处理不同类型的返回值
            if (typeof(T) == typeof(string))
            {
                return value.ToString() as T;
            }
            
            // 反序列化
            return Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting value from Redis for key {Key}", key);
            _statistics?.RecordMiss();
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var db = _redis.GetDatabase();
            var expiry = expiration ?? _options.Value.RedisCacheDefaultExpiration;
            
            // 处理null值
            if (value == null)
            {
                if (_options.Value.EnableNullValueCaching)
                {
                    await db.StringSetAsync(
                        key, 
                        "__NULL__", 
                        _options.Value.NullValueCacheTime);
                }
                return;
            }
            
            // 序列化
            string redisValue;
            if (typeof(T) == typeof(string) && value is string stringValue)
            {
                redisValue = stringValue;
            }
            else
            {
                redisValue = Serialize(value);
            }
            
            await db.StringSetAsync(key, redisValue, expiry);
            _statistics?.RecordSet();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error setting value in Redis for key {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking key existence in Redis for key {Key}", key);
            return false;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
            _statistics?.RecordRemove();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error removing key from Redis for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var db = _redis.GetDatabase();
            
            // 使用Lua脚本批量删除
            var result = await db.ScriptEvaluateAsync(
                CacheLuaScripts.DeleteByPrefix,
                new RedisKey[] { prefix });
            
            if (result.IsNull)
            {
                _logger?.LogWarning("Failed to remove keys by prefix {Prefix}", prefix);
            }
            else
            {
                var deletedCount = (int)result;
                _logger?.LogInformation("Deleted {Count} keys by prefix {Prefix}", deletedCount, prefix);
                
                // 记录删除操作
                for (int i = 0; i < deletedCount; i++)
                {
                    _statistics?.RecordRemove();
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error removing keys by prefix from Redis for prefix {Prefix}", prefix);
        }
    }

    /// <summary>
    /// 使用Lua脚本原子获取并刷新TTL
    /// </summary>
    public async Task<T?> GetAndRefreshAsync<T>(string key, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var db = _redis.GetDatabase();
            var ttl = (expiry ?? _options.Value.RedisCacheDefaultExpiration).TotalSeconds.ToString();
            
            var result = await db.ScriptEvaluateAsync(
                CacheLuaScripts.GetAndRefresh,
                new RedisKey[] { key },
                new RedisValue[] { ttl });
            
            if (result.IsNull)
            {
                _statistics?.RecordMiss();
                return null;
            }
            
            _statistics?.RecordHit();
            
            var value = result.ToString();
            if (value == "__NULL__")
            {
                return null;
            }
            
            return Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetAndRefreshAsync for key {Key}", key);
            _statistics?.RecordMiss();
            return null;
        }
    }

    /// <summary>
    /// 使用Lua脚本原子检查并设置
    /// </summary>
    public async Task<bool> CheckAndSetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            if (value == null)
            {
                return false;
            }

            var db = _redis.GetDatabase();
            var redisValue = Serialize(value);
            var ttl = (expiry ?? _options.Value.RedisCacheDefaultExpiration).TotalSeconds.ToString();
            
            var result = await db.ScriptEvaluateAsync(
                CacheLuaScripts.CheckAndSet,
                new RedisKey[] { key },
                new RedisValue[] { redisValue, ttl });
            
            var success = (int)result == 1;
            if (success)
            {
                _statistics?.RecordSet();
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in CheckAndSetAsync for key {Key}", key);
            return false;
        }
    }

    private string Serialize<T>(T value)
    {
        return _options.Value.SerializationType switch
        {
            CacheSerializationType.Json => JsonSerializer.Serialize(value),
            _ => JsonSerializer.Serialize(value) // 默认使用JSON
        };
    }

    private T? Deserialize<T>(string json)
    {
        try
        {
            return _options.Value.SerializationType switch
            {
                CacheSerializationType.Json => JsonSerializer.Deserialize<T>(json),
                _ => JsonSerializer.Deserialize<T>(json) // 默认使用JSON
            };
        }
        catch
        {
            return default(T);
        }
    }
}