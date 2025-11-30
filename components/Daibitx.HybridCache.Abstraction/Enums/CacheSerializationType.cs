namespace Daibitx.HybridCache.Abstraction.Enums;

/// <summary>
/// 缓存序列化类型
/// </summary>
public enum CacheSerializationType
{
    /// <summary>
    /// JSON序列化
    /// </summary>
    Json = 1,
    
    /// <summary>
    /// MessagePack序列化（高性能）
    /// </summary>
    MessagePack = 2,
    
    /// <summary>
    /// Protobuf序列化
    /// </summary>
    Protobuf = 3
}