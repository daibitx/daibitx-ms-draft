namespace Daibitx.HybridCache.Core.Constants;

/// <summary>
/// 缓存Lua脚本常量
/// </summary>
public static class CacheLuaScripts
{
    /// <summary>
    /// 原子获取并刷新TTL
    /// KEYS[1]: key
    /// ARGV[1]: ttl
    /// </summary>
    public const string GetAndRefresh = @"
        local key = KEYS[1]
        local ttl = ARGV[1]
        local value = redis.call('GET', key)
        if value then
            redis.call('EXPIRE', key, ttl)
        end
        return value";
    
    /// <summary>
    /// 原子检查并设置（防并发）
    /// KEYS[1]: key
    /// ARGV[1]: value
    /// ARGV[2]: ttl
    /// </summary>
    public const string CheckAndSet = @"
        local key = KEYS[1]
        local value = ARGV[1]
        local ttl = ARGV[2]
        local exists = redis.call('EXISTS', key)
        if exists == 0 then
            redis.call('SETEX', key, ttl, value)
            return 1
        end
        return 0";
    
    /// <summary>
    /// 批量删除（按前缀）
    /// KEYS[1]: prefix
    /// </summary>
    public const string DeleteByPrefix = @"
        local prefix = KEYS[1]
        local keys = redis.call('KEYS', prefix .. '*')
        local count = 0
        for i, key in ipairs(keys) do
            redis.call('DEL', key)
            count = count + 1
        end
        return count";
    
    /// <summary>
    /// 原子递增并设置过期时间
    /// KEYS[1]: key
    /// ARGV[1]: ttl
    /// </summary>
    public const string IncrementAndExpire = @"
        local key = KEYS[1]
        local ttl = ARGV[1]
        local value = redis.call('INCR', key)
        redis.call('EXPIRE', key, ttl)
        return value";
    
    /// <summary>
    /// 获取多个key的值
    /// KEYS: 多个key
    /// </summary>
    public const string MGet = @"
        local values = {}
        for i, key in ipairs(KEYS) do
            values[i] = redis.call('GET', key)
        end
        return values";
    
    /// <summary>
    /// 设置多个key的值并设置过期时间
    /// KEYS: 多个key
    /// ARGV: value1, ttl1, value2, ttl2, ...
    /// </summary>
    public const string MSetWithExpiry = @"
        local count = 0
        for i = 1, #KEYS do
            local key = KEYS[i]
            local value = ARGV[i * 2 - 1]
            local ttl = ARGV[i * 2]
            redis.call('SETEX', key, ttl, value)
            count = count + 1
        end
        return count";
}