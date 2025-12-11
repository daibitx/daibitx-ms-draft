using System.Net;
using Daibitx.HybridCache.Abstraction.Enums;
using Daibitx.HybridCache.Abstraction.Models;
using StackExchange.Redis;

namespace Daibitx.HybridCache.Redis.Extensions;

/// <summary>
/// Redis连接扩展
/// </summary>
public static class RedisConnectionExtensions
{
    /// <summary>
    /// 根据配置创建Redis连接
    /// </summary>
    public static IConnectionMultiplexer CreateConnection(RedisOptions options)
    {
        return options.Mode switch
        {
            RedisMode.Single => CreateSingleConnection(options),
            RedisMode.Sentinel => CreateSentinelConnection(options),
            RedisMode.Cluster => CreateClusterConnection(options),
            _ => throw new ArgumentException($"Unsupported Redis mode: {options.Mode}")
        };
    }

    private static IConnectionMultiplexer CreateSingleConnection(RedisOptions options)
    {
        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            throw new ArgumentException("Connection string is required for single mode");
        }

        var configuration = ConfigurationOptions.Parse(options.ConnectionString);
        ApplyCommonConfiguration(configuration, options);
        
        return ConnectionMultiplexer.Connect(configuration);
    }

    private static IConnectionMultiplexer CreateSentinelConnection(RedisOptions options)
    {
        if (options.Sentinel == null)
        {
            throw new ArgumentException("Sentinel configuration is required for sentinel mode");
        }

        if (options.Sentinel.Sentinels.Count == 0)
        {
            throw new ArgumentException("At least one sentinel is required");
        }

        if (string.IsNullOrEmpty(options.Sentinel.ServiceName))
        {
            throw new ArgumentException("Service name is required for sentinel mode");
        }

        var configuration = new ConfigurationOptions
        {
            ServiceName = options.Sentinel.ServiceName,
            CommandMap = CommandMap.Sentinel,
            TieBreaker = "",
            DefaultVersion = new Version(3, 0, 0),
            AllowAdmin = true
        };

        foreach (var sentinel in options.Sentinel.Sentinels)
        {
            configuration.EndPoints.Add(CreateEndPoint(sentinel));
        }

        if (!string.IsNullOrEmpty(options.Sentinel.Password))
        {
            configuration.Password = options.Sentinel.Password;
        }

        ApplyCommonConfiguration(configuration, options);

        // 连接到哨兵
        var sentinelConnection = ConnectionMultiplexer.Connect(configuration);
        
        // 获取主从连接
        var masterConfig = new ConfigurationOptions
        {
            ServiceName = options.Sentinel.ServiceName,
            CommandMap = CommandMap.Default,
            AllowAdmin = true
        };

        if (!string.IsNullOrEmpty(options.Password))
        {
            masterConfig.Password = options.Password;
        }

        ApplyCommonConfiguration(masterConfig, options);

        // 使用哨兵获取主从连接
        return sentinelConnection;
    }

    private static IConnectionMultiplexer CreateClusterConnection(RedisOptions options)
    {
        if (options.Cluster == null)
        {
            throw new ArgumentException("Cluster configuration is required for cluster mode");
        }

        if (options.Cluster.Nodes.Count == 0)
        {
            throw new ArgumentException("At least one cluster node is required");
        }

        var configuration = new ConfigurationOptions
        {
            CommandMap = CommandMap.Create(new HashSet<string>
            {
                "INFO", "CONFIG", "CLUSTER", "PING", "ECHO", "CLIENT"
            }, available: false),
            AllowAdmin = true
        };

        foreach (var node in options.Cluster.Nodes)
        {
            configuration.EndPoints.Add(CreateEndPoint(node));
        }

        if (!string.IsNullOrEmpty(options.Cluster.Password))
        {
            configuration.Password = options.Cluster.Password;
        }

        ApplyCommonConfiguration(configuration, options);

        return ConnectionMultiplexer.Connect(configuration);
    }

    private static void ApplyCommonConfiguration(ConfigurationOptions configuration, RedisOptions options)
    {
        configuration.ConnectTimeout = options.ConnectTimeout;
        configuration.AbortOnConnectFail = false;
        configuration.ConnectRetry = 3;
        configuration.DefaultDatabase = options.DefaultDatabase;
        configuration.Ssl = options.Ssl;
        configuration.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;

        if (!string.IsNullOrEmpty(options.Password))
        {
            configuration.Password = options.Password;
        }

        // 配置连接池
        configuration.SocketManager = SocketManager.ThreadPool;
    }

    private static EndPoint CreateEndPoint(string connectionString)
    {
        var parts = connectionString.Split(':');
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid endpoint format: {connectionString}. Expected format: host:port");
        }

        if (!int.TryParse(parts[1], out var port))
        {
            throw new ArgumentException($"Invalid port number: {parts[1]}");
        }

        return new DnsEndPoint(parts[0], port);
    }
}