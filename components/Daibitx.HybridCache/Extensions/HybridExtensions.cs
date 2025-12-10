using System.Reflection;
using Daibitx.HybridCache.Abstraction.Enums;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;
using Daibitx.HybridCache.Core.Implementations;
using Daibitx.HybridCache.Redis.Extensions;
using Daibitx.HybridCache.Redis.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Daibitx.HybridCache.Extensions;

/// <summary>
/// 多级缓存DI扩展
/// </summary>
public static class HybridCacheExtensions
{
    /// <summary>
    /// 添加多级缓存服务（从配置读取）
    /// </summary>
    public static IServiceCollection AddHybridCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 配置选项
        services.Configure<HybridCacheOptions>(options => options = configuration.GetSection("HybridCache").Get<HybridCacheOptions>());
        services.Configure<RedisOptions>(options => options = configuration.GetSection("Redis").Get<RedisOptions>());

        // 注册核心服务
        services.TryAddSingleton<ICacheStatistics, CacheStatistics>();
        services.TryAddSingleton<IHybridCache, Core.Implementations.HybridCache>();

        // 注册内存缓存
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheProvider, MemoryCacheProvider>();

        // 注册Redis相关服务
        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return RedisConnectionExtensions.CreateConnection(redisOptions);
        });

        services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
        services.TryAddSingleton<IRedLock, RedLock>();
        services.TryAddSingleton<ICacheSynchronizer, RedisCacheSynchronizer>();

        // 注册布隆过滤器（如果启用）
        services.TryAddSingleton<IBloomFilter>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<HybridCacheOptions>>().Value;
            if (options.EnableBloomFilter)
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                var logger = sp.GetService<ILogger<RedisBloomFilter>>();
                return new RedisBloomFilter(redis, Options.Create(options), logger);
            }
            return new NullBloomFilter();
        });

        return services;
    }

    /// <summary>
    /// 添加多级缓存服务（使用配置委托）
    /// </summary>
    public static IServiceCollection AddHybridCache(
        this IServiceCollection services,
        Action<HybridCacheOptions> configureOptions,
        Action<RedisOptions>? configureRedis = null)
    {
        // 配置选项
        services.Configure(configureOptions);

        if (configureRedis != null)
        {
            services.Configure(configureRedis);
        }

        // 注册核心服务
        services.TryAddSingleton<ICacheStatistics, CacheStatistics>();
        services.TryAddSingleton<IHybridCache, Core.Implementations.HybridCache>();

        // 注册内存缓存
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheProvider, MemoryCacheProvider>();

        // 注册Redis相关服务
        services.TryAddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisOptions = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return RedisConnectionExtensions.CreateConnection(redisOptions);
        });

        services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
        services.TryAddSingleton<IRedLock, RedLock>();
        services.TryAddSingleton<ICacheSynchronizer, RedisCacheSynchronizer>();

        // 注册布隆过滤器（如果启用）
        services.TryAddSingleton<IBloomFilter>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<HybridCacheOptions>>().Value;
            if (options.EnableBloomFilter)
            {
                var redis = sp.GetRequiredService<IConnectionMultiplexer>();
                var logger = sp.GetService<ILogger<RedisBloomFilter>>();
                return new RedisBloomFilter(redis, Options.Create(options), logger);
            }
            return new NullBloomFilter();
        });

        return services;
    }

    /// <summary>
    /// 添加多级缓存服务（自定义Redis连接）
    /// </summary>
    public static IServiceCollection AddHybridCache(
        this IServiceCollection services,
        Action<HybridCacheOptions> configureOptions,
        IConnectionMultiplexer redisConnection)
    {
        // 配置选项
        services.Configure(configureOptions);

        // 注册核心服务
        services.TryAddSingleton<ICacheStatistics, CacheStatistics>();
        services.TryAddSingleton<IHybridCache, Core.Implementations.HybridCache>();

        // 注册内存缓存
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheProvider, MemoryCacheProvider>();

        // 注册Redis相关服务
        services.TryAddSingleton(redisConnection);
        services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
        services.TryAddSingleton<IRedLock, RedLock>();
        services.TryAddSingleton<ICacheSynchronizer, RedisCacheSynchronizer>();

        // 注册布隆过滤器（如果启用）
        services.TryAddSingleton<IBloomFilter>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<HybridCacheOptions>>().Value;
            if (options.EnableBloomFilter)
            {
                var logger = sp.GetService<ILogger<RedisBloomFilter>>();
                return new RedisBloomFilter(redisConnection, Options.Create(options), logger);
            }
            return new NullBloomFilter();
        });

        return services;
    }

    /// <summary>
    /// 添加多级缓存服务（多Redis实例，用于RedLock）
    /// </summary>
    public static IServiceCollection AddHybridCache(
        this IServiceCollection services,
        Action<HybridCacheOptions> configureOptions,
        IEnumerable<IConnectionMultiplexer> redisConnections)
    {
        // 配置选项
        services.Configure(configureOptions);

        // 注册核心服务
        services.TryAddSingleton<ICacheStatistics, CacheStatistics>();
        services.TryAddSingleton<IHybridCache, Core.Implementations.HybridCache>();

        // 注册内存缓存
        services.AddMemoryCache();
        services.TryAddSingleton<ICacheProvider, MemoryCacheProvider>();

        // 注册Redis相关服务
        services.TryAddSingleton(redisConnections);
        services.TryAddSingleton<ICacheProvider, RedisCacheProvider>();
        services.TryAddSingleton<IRedLock>(sp =>
            new RedLock(
                sp.GetRequiredService<IEnumerable<IConnectionMultiplexer>>(),
                sp.GetService<ILogger<RedLock>>()));
        services.TryAddSingleton<ICacheSynchronizer, RedisCacheSynchronizer>();

        // 注册布隆过滤器（如果启用，使用第一个Redis连接）
        services.TryAddSingleton<IBloomFilter>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<HybridCacheOptions>>().Value;
            if (options.EnableBloomFilter)
            {
                var redisConnections = sp.GetRequiredService<IEnumerable<IConnectionMultiplexer>>();
                var firstRedis = redisConnections.FirstOrDefault();
                if (firstRedis != null)
                {
                    var logger = sp.GetService<ILogger<RedisBloomFilter>>();
                    return new RedisBloomFilter(firstRedis, Options.Create(options), logger);
                }
            }
            return new NullBloomFilter();
        });

        return services;
    }

    /// <summary>
    /// 安全模式配置（推荐用于生产环境）
    /// </summary>
    public static HybridCacheOptions AsSafeMode(this HybridCacheOptions options)
    {
        options.EnableNullValueCaching = true;
        options.EnableDistributedLock = true;
        options.EnableCacheSynchronization = true;
        options.EnableStatistics = true;
        return options;
    }

    /// <summary>
    /// 性能模式配置（推荐用于高并发场景）
    /// </summary>
    public static HybridCacheOptions AsPerformanceMode(this HybridCacheOptions options)
    {
        options.EnableMemoryCache = true;
        options.EnableRedisCache = true;
        options.MemoryCacheDefaultExpiration = TimeSpan.FromMinutes(10);
        options.RedisCacheDefaultExpiration = TimeSpan.FromHours(1);
        options.EnableDistributedLock = true;
        options.EnableCacheSynchronization = true;
        return options;
    }

    /// <summary>
    /// 开发模式配置（推荐用于开发环境）
    /// </summary>
    public static HybridCacheOptions AsDevelopmentMode(this HybridCacheOptions options)
    {
        options.EnableMemoryCache = true;
        options.EnableRedisCache = false; // 开发环境可以不使用Redis
        options.EnableNullValueCaching = true;
        options.EnableDistributedLock = false; // 开发环境可以不使用分布式锁
        options.EnableCacheSynchronization = false;
        options.EnableStatistics = true;
        return options;
    }

    /// <summary>
    /// 设置缓存Key前缀
    /// </summary>
    public static HybridCacheOptions WithKeyPrefix(this HybridCacheOptions options, string prefix)
    {
        options.KeyPrefix = prefix;
        return options;
    }

    /// <summary>
    /// 设置序列化方式
    /// </summary>
    public static HybridCacheOptions WithSerialization(this HybridCacheOptions options, CacheSerializationType serializationType)
    {
        options.SerializationType = serializationType;
        return options;
    }
}