using System.Text;
using System.Text.Json;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Daibitx.HybridCache.Redis.Implementations;

/// <summary>
/// RedLock分布式锁实现
/// </summary>
public class RedLock : IRedLock
{
    private readonly List<IConnectionMultiplexer> _redisInstances;
    private readonly ILogger<RedLock>? _logger;
    private const string LockValuePrefix = "redlock:";
    private const int DefaultRetryCount = 3;
    private const int DefaultRetryDelay = 200; // milliseconds

    public RedLock(
        IConnectionMultiplexer redis,
        ILogger<RedLock>? logger = null)
    {
        _redisInstances = new List<IConnectionMultiplexer> { redis };
        _logger = logger;
    }

    public RedLock(
        IEnumerable<IConnectionMultiplexer> redisInstances,
        ILogger<RedLock>? logger = null)
    {
        _redisInstances = redisInstances?.ToList() ?? throw new ArgumentNullException(nameof(redisInstances));
        _logger = logger;
        
        if (_redisInstances.Count == 0)
        {
            throw new ArgumentException("At least one Redis instance is required", nameof(redisInstances));
        }
    }

    public async Task<RedLockInstance?> AcquireAsync(
        string resource, 
        TimeSpan expiryTime,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var lockId = GenerateLockId();
        var validityTime = CalculateValidityTime(expiryTime);
        
        for (int attempt = 0; attempt < DefaultRetryCount; attempt++)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var lockedInstances = 0;

                // 尝试在所有Redis实例上获取锁
                foreach (var redis in _redisInstances)
                {
                    if (await TryAcquireLockAsync(redis, resource, lockId, expiryTime))
                    {
                        lockedInstances++;
                    }
                }

                // 计算获取锁所花费的时间
                var elapsedTime = DateTime.UtcNow - startTime;
                var remainingValidityTime = validityTime - elapsedTime;

                // 检查是否获得多数锁且锁仍然有效
                if (lockedInstances >= GetQuorum() && remainingValidityTime > TimeSpan.Zero)
                {
                    _logger?.LogDebug("Acquired RedLock for resource {Resource} with {Instances} instances", 
                        resource, lockedInstances);

                    return new RedLockInstance
                    {
                        Resource = resource,
                        LockId = lockId,
                        ValidUntil = DateTime.UtcNow + remainingValidityTime
                    };
                }

                // 获取锁失败，释放已获取的锁
                await ReleaseAsync(resource, lockId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error acquiring RedLock for resource {Resource}, attempt {Attempt}", 
                    resource, attempt + 1);
            }

            // 等待后重试
            if (attempt < DefaultRetryCount - 1)
            {
                await Task.Delay(DefaultRetryDelay, cancellationToken);
            }
        }

        _logger?.LogWarning("Failed to acquire RedLock for resource {Resource} after {Attempts} attempts", 
            resource, DefaultRetryCount);
        return null;
    }

    public async Task<bool> ExtendAsync(
        RedLockInstance lockInstance, 
        TimeSpan extensionTime,
        CancellationToken cancellationToken = default)
    {
        if (lockInstance == null)
        {
            throw new ArgumentNullException(nameof(lockInstance));
        }

        if (!lockInstance.IsValid)
        {
            _logger?.LogWarning("Cannot extend expired lock for resource {Resource}", lockInstance.Resource);
            return false;
        }

        try
        {
            var extendedInstances = 0;

            foreach (var redis in _redisInstances)
            {
                if (await ExtendLockAsync(redis, lockInstance.Resource, lockInstance.LockId, extensionTime))
                {
                    extendedInstances++;
                }
            }

            var success = extendedInstances >= GetQuorum();
            if (success)
            {
                lockInstance.ValidUntil = DateTime.UtcNow + extensionTime;
                _logger?.LogDebug("Extended RedLock for resource {Resource} with {Instances} instances", 
                    lockInstance.Resource, extendedInstances);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error extending RedLock for resource {Resource}", lockInstance.Resource);
            return false;
        }
    }

    public async Task ReleaseAsync(
        RedLockInstance lockInstance,
        CancellationToken cancellationToken = default)
    {
        if (lockInstance == null)
        {
            throw new ArgumentNullException(nameof(lockInstance));
        }

        await ReleaseAsync(lockInstance.Resource, lockInstance.LockId, cancellationToken);
    }

    public async Task<bool> IsAcquiredAsync(
        string resource,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var lockKey = GetLockKey(resource);
            var checkedInstances = 0;

            foreach (var redis in _redisInstances)
            {
                var db = redis.GetDatabase();
                var value = await db.StringGetAsync(lockKey);
                
                if (!value.IsNull)
                {
                    checkedInstances++;
                }
            }

            return checkedInstances >= GetQuorum();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error checking if lock is acquired for resource {Resource}", resource);
            return false;
        }
    }

    private async Task ReleaseAsync(string resource, string lockId, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = _redisInstances.Select(redis => ReleaseLockAsync(redis, resource, lockId));
            await Task.WhenAll(tasks);
            
            _logger?.LogDebug("Released RedLock for resource {Resource}", resource);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error releasing RedLock for resource {Resource}", resource);
        }
    }

    private async Task<bool> TryAcquireLockAsync(
        IConnectionMultiplexer redis, 
        string resource, 
        string lockId, 
        TimeSpan expiryTime)
    {
        try
        {
            var db = redis.GetDatabase();
            var lockKey = GetLockKey(resource);
            var lockValue = GetLockValue(lockId);
            
            // 使用SET NX PX命令原子性地设置锁
            return await db.StringSetAsync(
                lockKey, 
                lockValue, 
                expiryTime, 
                When.NotExists);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error acquiring lock on Redis instance for resource {Resource}", resource);
            return false;
        }
    }

    private async Task<bool> ExtendLockAsync(
        IConnectionMultiplexer redis, 
        string resource, 
        string lockId, 
        TimeSpan extensionTime)
    {
        try
        {
            var db = redis.GetDatabase();
            var lockKey = GetLockKey(resource);
            var lockValue = GetLockValue(lockId);
            
            // 检查锁是否存在且值匹配
            var currentValue = await db.StringGetAsync(lockKey);
            if (currentValue != lockValue)
            {
                return false; // 锁不存在或已被其他客户端获取
            }

            // 延长锁的过期时间
            return await db.KeyExpireAsync(lockKey, extensionTime);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error extending lock on Redis instance for resource {Resource}", resource);
            return false;
        }
    }

    private async Task ReleaseLockAsync(
        IConnectionMultiplexer redis, 
        string resource, 
        string lockId)
    {
        try
        {
            var db = redis.GetDatabase();
            var lockKey = GetLockKey(resource);
            var lockValue = GetLockValue(lockId);
            
            // 使用Lua脚本原子性地删除锁
            var script = @"
                local key = KEYS[1]
                local value = ARGV[1]
                local current = redis.call('GET', key)
                if current == value then
                    redis.call('DEL', key)
                    return 1
                end
                return 0";
            
            await db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { lockValue });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error releasing lock on Redis instance for resource {Resource}", resource);
        }
    }

    private string GetLockKey(string resource)
    {
        return $"redlock:{resource}";
    }

    private string GetLockValue(string lockId)
    {
        return $"{LockValuePrefix}{lockId}";
    }

    private string GenerateLockId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private TimeSpan CalculateValidityTime(TimeSpan expiryTime)
    {
        // 减去时钟漂移和获取锁的时间
        var drift = expiryTime.TotalMilliseconds * 0.01; // 1%的时钟漂移
        return TimeSpan.FromMilliseconds(expiryTime.TotalMilliseconds - drift);
    }

    private int GetQuorum()
    {
        return _redisInstances.Count / 2 + 1;
    }
}