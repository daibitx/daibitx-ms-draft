namespace Daibitx.HybridCache.Abstraction.Models;

/// <summary>
/// RedLock锁实例
/// </summary>
public class RedLockInstance
{
    /// <summary>
    /// 锁的资源名称
    /// </summary>
    public string Resource { get; set; } = string.Empty;
    
    /// <summary>
    /// 锁的唯一标识
    /// </summary>
    public string LockId { get; set; } = string.Empty;
    
    /// <summary>
    /// 锁的有效截止时间
    /// </summary>
    public DateTime ValidUntil { get; set; }
    
    /// <summary>
    /// 锁是否有效
    /// </summary>
    public bool IsValid => DateTime.UtcNow < ValidUntil;
    
    /// <summary>
    /// 锁的剩余有效时间
    /// </summary>
    public TimeSpan RemainingTime => ValidUntil > DateTime.UtcNow ? ValidUntil - DateTime.UtcNow : TimeSpan.Zero;
}