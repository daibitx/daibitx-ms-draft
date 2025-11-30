namespace Daibitx.HybridCache.Abstraction.Interfaces;

/// <summary>
/// 缓存同步接口
/// </summary>
public interface ICacheSynchronizer
{
    /// <summary>
    /// 发布缓存失效消息
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishInvalidateAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发布前缀缓存失效消息
    /// </summary>
    /// <param name="prefix">缓存键前缀</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task PublishInvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 订阅缓存同步消息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task SubscribeAsync(CancellationToken cancellationToken = default);
}