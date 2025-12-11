using System.Text.Json;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Daibitx.HybridCache.Redis.Implementations;

/// <summary>
/// Redis缓存同步实现
/// </summary>
public class RedisCacheSynchronizer : ICacheSynchronizer, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IOptions<HybridCacheOptions> _options;
    private readonly ILogger<RedisCacheSynchronizer>? _logger;
    private readonly IMemoryCache? _memoryCache;
    private ISubscriber? _subscriber;
    private bool _disposed;

    public RedisCacheSynchronizer(
        IConnectionMultiplexer redis,
        IOptions<HybridCacheOptions> options,
        ILogger<RedisCacheSynchronizer>? logger = null,
        IMemoryCache? memoryCache = null)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task PublishInvalidateAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var channel = _options.Value.SynchronizationChannel;
            var message = new CacheSyncMessage
            {
                Type = CacheSyncType.Invalidate,
                Key = key,
                Timestamp = DateTime.UtcNow
            };

            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(
                RedisChannel.Literal(channel), 
                JsonSerializer.Serialize(message));
            
            _logger?.LogDebug("Published invalidate message for key {Key} to channel {Channel}", key, channel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error publishing invalidate message for key {Key}", key);
        }
    }

    public async Task PublishInvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var channel = _options.Value.SynchronizationChannel;
            var message = new CacheSyncMessage
            {
                Type = CacheSyncType.InvalidateByPrefix,
                Key = prefix,
                Timestamp = DateTime.UtcNow
            };

            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(
                RedisChannel.Literal(channel), 
                JsonSerializer.Serialize(message));
            
            _logger?.LogDebug("Published invalidate by prefix message for prefix {Prefix} to channel {Channel}", prefix, channel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error publishing invalidate by prefix message for prefix {Prefix}", prefix);
        }
    }

    public async Task SubscribeAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Value.EnableCacheSynchronization)
        {
            return;
        }

        try
        {
            var channel = _options.Value.SynchronizationChannel;
            _subscriber = _redis.GetSubscriber();
            
            await _subscriber.SubscribeAsync(
                RedisChannel.Literal(channel), 
                (redisChannel, value) => HandleSyncMessage(value));
            
            _logger?.LogInformation("Subscribed to cache synchronization channel {Channel}", channel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error subscribing to cache synchronization channel");
        }
    }

    private void HandleSyncMessage(RedisValue value)
    {
        try
        {
            var message = JsonSerializer.Deserialize<CacheSyncMessage>(value.ToString());
            if (message == null)
            {
                return;
            }

            // 忽略自己发送的消息（可以通过实例ID判断）
            var timeDiff = DateTime.UtcNow - message.Timestamp;
            if (timeDiff.TotalSeconds > 60) // 忽略60秒前的消息
            {
                return;
            }

            switch (message.Type)
            {
                case CacheSyncType.Invalidate:
                    InvalidateKey(message.Key);
                    break;
                case CacheSyncType.InvalidateByPrefix:
                    InvalidateKeysByPrefix(message.Key);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling sync message");
        }
    }

    private void InvalidateKey(string key)
    {
        if (_memoryCache != null)
        {
            _memoryCache.Remove(key);
            _logger?.LogDebug("Invalidated memory cache for key {Key}", key);
        }
    }

    private void InvalidateKeysByPrefix(string prefix)
    {
        if (_memoryCache == null)
        {
            return;
        }

        // 内存缓存不支持直接按前缀删除，这里只是记录日志
        // 实际项目中可以考虑使用缓存条目链接或定期清理
        _logger?.LogDebug("Invalidate by prefix requested for prefix {Prefix}", prefix);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_subscriber != null)
            {
                try
                {
                    var channel = _options.Value.SynchronizationChannel;
                    _subscriber.Unsubscribe(RedisChannel.Literal(channel));
                    _logger?.LogInformation("Unsubscribed from cache synchronization channel {Channel}", channel);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error unsubscribing from cache synchronization channel");
                }
            }
            
            _disposed = true;
        }
    }
}

/// <summary>
/// 缓存同步消息
/// </summary>
internal class CacheSyncMessage
{
    public CacheSyncType Type { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 缓存同步类型
/// </summary>
internal enum CacheSyncType
{
    Invalidate,
    InvalidateByPrefix
}