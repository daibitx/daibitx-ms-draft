using Daibitx.HybridCache.Abstraction.Models;

namespace Daibitx.HybridCache.Abstraction.Interfaces;

/// <summary>
/// RedLock分布式锁接口
/// </summary>
public interface IRedLock
{
    /// <summary>
    /// 获取分布式锁
    /// </summary>
    /// <param name="resource">锁定的资源名称</param>
    /// <param name="expiryTime">锁的过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>锁实例（获取成功）或null（获取失败）</returns>
    Task<RedLockInstance?> AcquireAsync(
        string resource, 
        TimeSpan expiryTime,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 续期分布式锁
    /// </summary>
    /// <param name="lockInstance">锁实例</param>
    /// <param name="extensionTime">续期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否续期成功</returns>
    Task<bool> ExtendAsync(
        RedLockInstance lockInstance, 
        TimeSpan extensionTime,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 释放分布式锁
    /// </summary>
    /// <param name="lockInstance">锁实例</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task ReleaseAsync(
        RedLockInstance lockInstance,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查锁是否被持有
    /// </summary>
    /// <param name="resource">资源名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否被持有</returns>
    Task<bool> IsAcquiredAsync(
        string resource,
        CancellationToken cancellationToken = default);
}