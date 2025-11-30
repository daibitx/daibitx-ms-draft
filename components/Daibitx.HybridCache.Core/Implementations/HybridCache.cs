using System.Text;
using System.Text.Json;
using Daibitx.HybridCache.Abstraction.Enums;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Daibitx.HybridCache.Core.Implementations;

/// <summary>
/// 多级缓存主实现
/// </summary>
public class HybridCache : IHybridCache
{
    private readonly IEnumerable<ICacheProvider> _cacheProviders;
    private readonly ICacheStatistics? _statistics;
    private readonly IRedLock? _redLock;
    private readonly ICacheSynchronizer? _synchronizer;
    private readonly IOptions<HybridCacheOptions> _options;
    private readonly ILogger<HybridCache>? _logger;

    public HybridCache(
        IEnumerable<ICacheProvider> cacheProviders,
        IOptions<HybridCacheOptions> options,
        ICacheStatistics? statistics = null,
        IRedLock? redLock = null,
        ICacheSynchronizer? synchronizer = null,
        ILogger<HybridCache>? logger = null)
    {
        _cacheProviders = cacheProviders ?? throw new ArgumentNullException(nameof(cacheProviders));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _statistics = statistics;
        _redLock = redLock;
        _synchronizer = synchronizer;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        HybridCacheOptions? options = null, 
        CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var cacheOptions = options ?? _options.Value;
        var cacheKey = BuildCacheKey(key, cacheOptions);
        
        // 尝试从缓存获取
        var cachedValue = await GetAsync<T>(cacheKey, cancellationToken);
        if (cachedValue != null)
        {
            return cachedValue;
        }
        
        // 检查是否是空值缓存
        if (cacheOptions.EnableNullValueCaching && await IsNullValueCachedAsync(cacheKey, cancellationToken))
        {
            return null;
        }
        
        // 获取分布式锁（如果启用）
        var lockKey = $"lock:{cacheKey}";
        RedLockInstance? lockInstance = null;
        
        try
        {
            if (cacheOptions.EnableDistributedLock && _redLock != null)
            {
                lockInstance = await _redLock.AcquireAsync(
                    lockKey, 
                    cacheOptions.LockDefaultExpiration, 
                    cancellationToken);
                
                // 获取锁后再次检查缓存
                if (lockInstance != null)
                {
                    cachedValue = await GetAsync<T>(cacheKey, cancellationToken);
                    if (cachedValue != null)
                    {
                        return cachedValue;
                    }
                }
            }
            
            // 执行工厂函数获取数据
            var value = await factory();
            
            // 设置缓存
            await SetAsync(cacheKey, value, cacheOptions, cancellationToken);
            
            return value;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetOrCreateAsync for key {Key}", key);
            throw;
        }
        finally
        {
            // 释放锁
            if (lockInstance != null && _redLock != null)
            {
                try
                {
                    await _redLock.ReleaseAsync(lockInstance, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error releasing lock for key {Key}", key);
                }
            }
        }
    }

    public async Task SetAsync<T>(
        string key, 
        T value, 
        HybridCacheOptions? options = null, 
        CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var cacheOptions = options ?? _options.Value;
        var cacheKey = BuildCacheKey(key, cacheOptions);
        
        try
        {
            // 设置内存缓存
            if (cacheOptions.EnableMemoryCache)
            {
                var memoryProvider = GetProvider("MemoryCache");
                if (memoryProvider != null)
                {
                    var memoryExpiration = GetMemoryExpiration(cacheOptions);
                    await memoryProvider.SetAsync(cacheKey, value, memoryExpiration, cancellationToken);
                }
            }
            
            // 设置Redis缓存
            if (cacheOptions.EnableRedisCache)
            {
                var redisProvider = GetProvider("RedisCache");
                if (redisProvider != null)
                {
                    var redisExpiration = GetRedisExpiration(cacheOptions);
                    await redisProvider.SetAsync(cacheKey, value, redisExpiration, cancellationToken);
                }
            }
            
            // 发布缓存更新消息
            if (cacheOptions.EnableCacheSynchronization && _synchronizer != null)
            {
                await _synchronizer.PublishInvalidateAsync(cacheKey, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in SetAsync for key {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var cacheKey = BuildCacheKey(key, _options.Value);
        
        try
        {
            // 删除内存缓存
            if (_options.Value.EnableMemoryCache)
            {
                var memoryProvider = GetProvider("MemoryCache");
                if (memoryProvider != null)
                {
                    await memoryProvider.RemoveAsync(cacheKey, cancellationToken);
                }
            }
            
            // 删除Redis缓存
            if (_options.Value.EnableRedisCache)
            {
                var redisProvider = GetProvider("RedisCache");
                if (redisProvider != null)
                {
                    await redisProvider.RemoveAsync(cacheKey, cancellationToken);
                }
            }
            
            // 发布缓存失效消息
            if (_options.Value.EnableCacheSynchronization && _synchronizer != null)
            {
                await _synchronizer.PublishInvalidateAsync(cacheKey, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in RemoveAsync for key {Key}", key);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var cacheOptions = _options.Value;
        var cacheKey = BuildCacheKey(key, cacheOptions);
        
        try
        {
            // 优先从内存缓存获取
            if (cacheOptions.EnableMemoryCache)
            {
                var memoryProvider = GetProvider("MemoryCache");
                if (memoryProvider != null)
                {
                    var value = await memoryProvider.GetAsync<T>(cacheKey, cancellationToken);
                    if (value != null)
                    {
                        _statistics?.RecordHit();
                        return value;
                    }
                }
            }
            
            // 从Redis缓存获取
            if (cacheOptions.EnableRedisCache)
            {
                var redisProvider = GetProvider("RedisCache");
                if (redisProvider != null)
                {
                    var value = await redisProvider.GetAsync<T>(cacheKey, cancellationToken);
                    if (value != null)
                    {
                        _statistics?.RecordHit();
                        
                        // 回填内存缓存
                        if (cacheOptions.EnableMemoryCache)
                        {
                            var memoryProvider = GetProvider("MemoryCache");
                            if (memoryProvider != null)
                            {
                                var memoryExpiration = GetMemoryExpiration(cacheOptions);
                                await memoryProvider.SetAsync(cacheKey, value, memoryExpiration, cancellationToken);
                            }
                        }
                        
                        return value;
                    }
                }
            }
            
            _statistics?.RecordMiss();
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetAsync for key {Key}", key);
            _statistics?.RecordMiss();
            return null;
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var cachePrefix = BuildCacheKey(prefix, _options.Value);
        
        try
        {
            // 删除内存缓存
            if (_options.Value.EnableMemoryCache)
            {
                var memoryProvider = GetProvider("MemoryCache");
                if (memoryProvider != null)
                {
                    await memoryProvider.RemoveByPrefixAsync(cachePrefix, cancellationToken);
                }
            }
            
            // 删除Redis缓存
            if (_options.Value.EnableRedisCache)
            {
                var redisProvider = GetProvider("RedisCache");
                if (redisProvider != null)
                {
                    await redisProvider.RemoveByPrefixAsync(cachePrefix, cancellationToken);
                }
            }
            
            // 发布缓存失效消息
            if (_options.Value.EnableCacheSynchronization && _synchronizer != null)
            {
                await _synchronizer.PublishInvalidateByPrefixAsync(cachePrefix, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in RemoveByPrefixAsync for prefix {Prefix}", prefix);
            throw;
        }
    }

    private ICacheProvider? GetProvider(string name)
    {
        return _cacheProviders.FirstOrDefault(p => p.Name == name);
    }

    private string BuildCacheKey(string key, HybridCacheOptions options)
    {
        if (string.IsNullOrEmpty(options.KeyPrefix))
        {
            return key;
        }
        
        return $"{options.KeyPrefix}:{key}";
    }

    private TimeSpan GetMemoryExpiration(HybridCacheOptions options)
    {
        // 添加随机因子防止缓存雪崩
        var baseExpiration = options.MemoryCacheDefaultExpiration;
        var random = new Random();
        var variance = baseExpiration.TotalSeconds * 0.1; // 10%的随机 variance
        var randomSeconds = baseExpiration.TotalSeconds + random.NextDouble() * variance;
        
        return TimeSpan.FromSeconds(randomSeconds);
    }

    private TimeSpan GetRedisExpiration(HybridCacheOptions options)
    {
        // 添加随机因子防止缓存雪崩
        var baseExpiration = options.RedisCacheDefaultExpiration;
        var random = new Random();
        var variance = baseExpiration.TotalSeconds * 0.1; // 10%的随机 variance
        var randomSeconds = baseExpiration.TotalSeconds + random.NextDouble() * variance;
        
        return TimeSpan.FromSeconds(randomSeconds);
    }

    private async Task<bool> IsNullValueCachedAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            var memoryProvider = GetProvider("MemoryCache");
            if (memoryProvider != null && await memoryProvider.GetAsync<string>(key, cancellationToken) == "__NULL__")
            {
                return true;
            }

            var redisProvider = GetProvider("RedisCache");
            if (redisProvider != null && await redisProvider.GetAsync<string>(key, cancellationToken) == "__NULL__")
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}