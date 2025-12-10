namespace Daibitx.HybridCache.Abstraction.Interfaces;

/// <summary>
/// 布隆过滤器接口
/// </summary>
public interface IBloomFilter
{
    /// <summary>
    /// 布隆过滤器名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 添加元素到布隆过滤器
    /// </summary>
    /// <param name="key">元素键</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task AddAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 批量添加元素到布隆过滤器
    /// </summary>
    /// <param name="keys">元素键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task AddAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查元素是否可能存在于布隆过滤器中
    /// </summary>
    /// <param name="key">元素键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>true表示可能存在，false表示一定不存在</returns>
    Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 批量检查元素是否存在
    /// </summary>
    /// <param name="keys">元素键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>可能存在的结果集合</returns>
    Task<IEnumerable<bool>> ContainsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 清空布隆过滤器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task ClearAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取布隆过滤器统计信息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>统计信息</returns>
    Task<BloomFilterStats> GetStatsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 布隆过滤器统计信息
/// </summary>
public class BloomFilterStats
{
    /// <summary>
    /// 预期元素数量
    /// </summary>
    public long ExpectedElements { get; set; }
    
    /// <summary>
    /// 实际元素数量（估算）
    /// </summary>
    public long ApproximateCount { get; set; }
    
    /// <summary>
    /// Bitmap 大小（位）
    /// </summary>
    public long BitmapSize { get; set; }
    
    /// <summary>
    /// 哈希函数数量
    /// </summary>
    public int HashFunctionCount { get; set; }
    
    /// <summary>
    /// 误判率
    /// </summary>
    public double FalsePositiveRate { get; set; }
    
    /// <summary>
    /// 已设置的位数
    /// </summary>
    public long SetBitsCount { get; set; }
}