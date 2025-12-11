namespace Daibitx.HybridCache.Abstraction.Enums;

/// <summary>
/// Redis部署模式
/// </summary>
public enum RedisMode
{
    /// <summary>
    /// 单点模式
    /// </summary>
    Single = 1,
    
    /// <summary>
    /// 哨兵模式（主从+Sentinel）
    /// </summary>
    Sentinel = 2,
    
    /// <summary>
    /// 集群模式（Cluster）
    /// </summary>
    Cluster = 3
}