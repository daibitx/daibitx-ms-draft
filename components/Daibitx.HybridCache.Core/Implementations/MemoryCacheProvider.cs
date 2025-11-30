using System.Text.Json;
using Daibitx.HybridCache.Abstraction.Enums;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Daibitx.HybridCache.Core.Implementations;

/// <summary>
/// 内存缓存提供者实现
/// </summary>
public class MemoryCacheProvider : ICacheProvider, IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<HybridCacheOptions> _options;
    private readonly ICacheStatistics? _statistics;
    private bool _disposed;

    public string Name => "MemoryCache";

    public MemoryCacheProvider(
        IMemoryCache memoryCache,
        IOptions<HybridCacheOptions> options,
        ICacheStatistics? statistics = null)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _statistics = statistics;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            if (_memoryCache.TryGetValue(key, out var value))
            {
                _statistics?.RecordHit();
                
                // 处理空值缓存
                if (value is string strValue && strValue == "__NULL__")
                {
                    return null;
                }
                
                // 处理不同类型的返回值
                return value switch
                {
                    T typedValue => typedValue,
                    string json when typeof(T) != typeof(string) => Deserialize<T>(json),
                    _ => value as T
                };
            }
            
            _statistics?.RecordMiss();
            return null;
        }
        catch (Exception)
        {
            _statistics?.RecordMiss();
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? _options.Value.MemoryCacheDefaultExpiration
            };

            // 处理null值
            if (value == null)
            {
                if (_options.Value.EnableNullValueCaching)
                {
                    _memoryCache.Set(key, "__NULL__", options);
                }
                return;
            }

            // 如果是字符串类型，直接存储
            if (typeof(T) == typeof(string) && value is string stringValue)
            {
                _memoryCache.Set(key, stringValue, options);
            }
            else
            {
                // 序列化后存储
                var json = Serialize(value);
                _memoryCache.Set(key, json, options);
            }

            _statistics?.RecordSet();
        }
        catch (Exception)
        {
            // 内存缓存失败不影响主流程
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _memoryCache.TryGetValue(key, out _);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _memoryCache.Remove(key);
        _statistics?.RecordRemove();
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        // 内存缓存不支持按前缀删除，这里只是模拟
        // 实际项目中可以考虑使用缓存条目链接或定期清理
        _statistics?.RecordRemove();
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

    public void Dispose()
    {
        if (!_disposed)
        {
            _memoryCache.Dispose();
            _disposed = true;
        }
    }
}