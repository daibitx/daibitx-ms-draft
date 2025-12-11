using Daibitx.HybridCache.Abstraction.Enums;

namespace Daibitx.HybridCache.Abstraction.Models;

/// <summary>
/// 多级缓存配置选项
/// </summary>
public class HybridCacheOptions
{
    /// <summary>
    /// 是否启用内存缓存
    /// </summary>
    public bool EnableMemoryCache { get; set; } = true;
    
    /// <summary>
    /// 是否启用Redis缓存
    /// </summary>
    public bool EnableRedisCache { get; set; } = true;
    
    /// <summary>
    /// 内存缓存默认过期时间
    /// </summary>
    public TimeSpan MemoryCacheDefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Redis缓存默认过期时间
    /// </summary>
    public TimeSpan RedisCacheDefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// 空值缓存时间（防穿透）
    /// </summary>
    public TimeSpan NullValueCacheTime { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// 是否启用空值缓存
    /// </summary>
    public bool EnableNullValueCaching { get; set; } = true;
    
    /// <summary>
    /// 是否启用分布式锁
    /// </summary>
    public bool EnableDistributedLock { get; set; } = true;
    
    /// <summary>
    /// 锁的默认过期时间
    /// </summary>
    public TimeSpan LockDefaultExpiration { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    /// 是否启用缓存同步
    /// </summary>
    public bool EnableCacheSynchronization { get; set; } = true;
    
    /// <summary>
    /// 同步通道名称
    /// </summary>
    public string SynchronizationChannel { get; set; } = "daibitx:cache:sync";
    
    /// <summary>
    /// 是否启用统计
    /// </summary>
    public bool EnableStatistics { get; set; } = true;
    
    /// <summary>
    /// 序列化方式
    /// </summary>
    public CacheSerializationType SerializationType { get; set; } = CacheSerializationType.Json;
    
    /// <summary>
    /// 缓存Key前缀
    /// </summary>
    public string KeyPrefix { get; set; } = "";
    
    /// <summary>
    /// 是否启用Key哈希
    /// </summary>
    public bool EnableKeyHashing { get; set; } = false;
    
    /// <summary>
    /// 是否启用布隆过滤器（防缓存穿透）
    /// </summary>
    public bool EnableBloomFilter { get; set; } = false;
    
    /// <summary>
    /// 布隆过滤器配置
    /// </summary>
    public BloomFilterOptions BloomFilter { get; set; } = new();
}

/// <summary>
/// 布隆过滤器配置选项
/// </summary>
public class BloomFilterOptions
{
    /// <summary>
    /// 预期元素数量（默认100万）
    /// </summary>
    public long ExpectedElements { get; set; } = 1_000_000;
    
    /// <summary>
    /// 误判率（默认0.01%）
    /// </summary>
    public double FalsePositiveRate { get; set; } = 0.0001;
    
    /// <summary>
    /// 布隆过滤器Key前缀
    /// </summary>
    public string KeyPrefix { get; set; } = "bloomfilter";
    
    /// <summary>
    /// 是否自动构建（渐进式）
    /// </summary>
    public bool AutoBuild { get; set; } = true;
    
    /// <summary>
    /// 重建布隆过滤器的间隔时间
    /// </summary>
    public TimeSpan RebuildInterval { get; set; } = TimeSpan.FromHours(24);
}