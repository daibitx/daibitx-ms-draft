namespace Daibitx.HybridCache.Abstraction.Models;

/// <summary>
/// 缓存性能指标
/// </summary>
public class CacheMetrics
{
    /// <summary>
    /// 总请求数
    /// </summary>
    public long TotalRequests { get; set; }
    
    /// <summary>
    /// 缓存命中数
    /// </summary>
    public long CacheHits { get; set; }
    
    /// <summary>
    /// 缓存未命中数
    /// </summary>
    public long CacheMisses { get; set; }
    
    /// <summary>
    /// 缓存命中率（百分比）
    /// </summary>
    public double HitRate { get; set; }
    
    /// <summary>
    /// 设置操作数
    /// </summary>
    public long Sets { get; set; }
    
    /// <summary>
    /// 删除操作数
    /// </summary>
    public long Removes { get; set; }
    
    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    public double AverageResponseTime { get; set; }
    
    /// <summary>
    /// 内存缓存条目数
    /// </summary>
    public long MemoryCacheEntries { get; set; }
    
    /// <summary>
    /// Redis缓存条目数
    /// </summary>
    public long RedisCacheEntries { get; set; }
    
    /// <summary>
    /// 统计开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// 统计结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
}