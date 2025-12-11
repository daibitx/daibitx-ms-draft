using Daibitx.HybridCache.Abstraction.Models;

namespace Daibitx.HybridCache.Abstraction.Interfaces;

/// <summary>
/// 多级缓存主接口
/// </summary>
public interface IHybridCache
{
    /// <summary>
    /// 获取或创建缓存项
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="factory">缓存项工厂函数</param>
    /// <param name="options">缓存选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值</returns>
    Task<T?> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        HybridCacheOptions? options = null, 
        CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// 设置缓存项
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="options">缓存选项</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task SetAsync<T>(
        string key, 
        T value, 
        HybridCacheOptions? options = null, 
        CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// 删除缓存项
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task RemoveAsync(
        string key, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取缓存项
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值</returns>
    Task<T?> GetAsync<T>(
        string key, 
        CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// 根据前缀删除缓存项
    /// </summary>
    /// <param name="prefix">缓存键前缀</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task RemoveByPrefixAsync(
        string prefix, 
        CancellationToken cancellationToken = default);
}