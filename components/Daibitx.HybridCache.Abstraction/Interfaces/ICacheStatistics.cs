using Daibitx.HybridCache.Abstraction.Models;

namespace Daibitx.HybridCache.Abstraction.Interfaces;

/// <summary>
/// 缓存统计接口
/// </summary>
public interface ICacheStatistics
{
    /// <summary>
    /// 总请求数
    /// </summary>
    long TotalRequests { get; }
    
    /// <summary>
    /// 缓存命中数
    /// </summary>
    long CacheHits { get; }
    
    /// <summary>
    /// 缓存未命中数
    /// </summary>
    long CacheMisses { get; }
    
    /// <summary>
    /// 缓存命中率（百分比）
    /// </summary>
    double HitRate { get; }
    
    /// <summary>
    /// 记录缓存命中
    /// </summary>
    void RecordHit();
    
    /// <summary>
    /// 记录缓存未命中
    /// </summary>
    void RecordMiss();
    
    /// <summary>
    /// 记录设置操作
    /// </summary>
    void RecordSet();
    
    /// <summary>
    /// 记录删除操作
    /// </summary>
    void RecordRemove();
    
    /// <summary>
    /// 获取当前指标
    /// </summary>
    /// <returns>缓存指标</returns>
    CacheMetrics GetMetrics();
    
    /// <summary>
    /// 重置统计
    /// </summary>
    void Reset();
}