using System.Threading;
using Daibitx.HybridCache.Abstraction.Interfaces;
using Daibitx.HybridCache.Abstraction.Models;

namespace Daibitx.HybridCache.Core.Implementations;

/// <summary>
/// 缓存统计实现
/// </summary>
public class CacheStatistics : ICacheStatistics
{
    private long _totalRequests;
    private long _cacheHits;
    private long _cacheMisses;
    private long _sets;
    private long _removes;
    private readonly DateTime _startTime;
    
    public CacheStatistics()
    {
        _startTime = DateTime.UtcNow;
    }
    
    /// <inheritdoc />
    public long TotalRequests => _totalRequests;
    
    /// <inheritdoc />
    public long CacheHits => _cacheHits;
    
    /// <inheritdoc />
    public long CacheMisses => _cacheMisses;
    
    /// <inheritdoc />
    public double HitRate
    {
        get
        {
            var total = _cacheHits + _cacheMisses;
            return total > 0 ? (double)_cacheHits / total * 100 : 0;
        }
    }
    
    /// <inheritdoc />
    public void RecordHit()
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _cacheHits);
    }
    
    /// <inheritdoc />
    public void RecordMiss()
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Increment(ref _cacheMisses);
    }
    
    /// <inheritdoc />
    public void RecordSet()
    {
        Interlocked.Increment(ref _sets);
    }
    
    /// <inheritdoc />
    public void RecordRemove()
    {
        Interlocked.Increment(ref _removes);
    }
    
    /// <inheritdoc />
    public CacheMetrics GetMetrics()
    {
        return new CacheMetrics
        {
            TotalRequests = _totalRequests,
            CacheHits = _cacheHits,
            CacheMisses = _cacheMisses,
            HitRate = HitRate,
            Sets = _sets,
            Removes = _removes,
            StartTime = _startTime,
            EndTime = DateTime.UtcNow
        };
    }
    
    /// <inheritdoc />
    public void Reset()
    {
        Interlocked.Exchange(ref _totalRequests, 0);
        Interlocked.Exchange(ref _cacheHits, 0);
        Interlocked.Exchange(ref _cacheMisses, 0);
        Interlocked.Exchange(ref _sets, 0);
        Interlocked.Exchange(ref _removes, 0);
    }
}