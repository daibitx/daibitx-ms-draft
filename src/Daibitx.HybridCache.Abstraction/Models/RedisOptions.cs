using Daibitx.HybridCache.Abstraction.Enums;

namespace Daibitx.HybridCache.Abstraction.Models;

/// <summary>
/// Redis配置选项
/// </summary>
public class RedisOptions
{
    /// <summary>
    /// Redis部署模式
    /// </summary>
    public RedisMode Mode { get; set; } = RedisMode.Single;
    
    /// <summary>
    /// 连接字符串（单点模式）
    /// </summary>
    public string? ConnectionString { get; set; }
    
    /// <summary>
    /// 哨兵配置
    /// </summary>
    public RedisSentinelOptions? Sentinel { get; set; }
    
    /// <summary>
    /// 集群配置
    /// </summary>
    public RedisClusterOptions? Cluster { get; set; }
    
    /// <summary>
    /// 连接池大小
    /// </summary>
    public int ConnectionPoolSize { get; set; } = 50;
    
    /// <summary>
    /// 连接超时（毫秒）
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;
    
    /// <summary>
    /// 是否启用SSL
    /// </summary>
    public bool Ssl { get; set; } = false;
    
    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// 默认数据库
    /// </summary>
    public int DefaultDatabase { get; set; } = 0;
}

/// <summary>
/// Redis哨兵配置
/// </summary>
public class RedisSentinelOptions
{
    /// <summary>
    /// 哨兵节点列表
    /// </summary>
    public List<string> Sentinels { get; set; } = new();
    
    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; } = "";
    
    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// Redis集群配置
/// </summary>
public class RedisClusterOptions
{
    /// <summary>
    /// 集群节点列表
    /// </summary>
    public List<string> Nodes { get; set; } = new();
    
    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }
}