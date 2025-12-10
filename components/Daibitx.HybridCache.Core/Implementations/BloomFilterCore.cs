using System.Security.Cryptography;
using System.Text;
using Daibitx.HybridCache.Abstraction.Interfaces;

namespace Daibitx.HybridCache.Core.Implementations;

/// <summary>
/// 布隆过滤器核心算法实现
/// </summary>
public class BloomFilterCore
{
    private readonly long _bitmapSize;
    private readonly int _hashFunctionCount;
    private readonly long _expectedElements;
    private readonly double _falsePositiveRate;

    /// <summary>
    /// 初始化布隆过滤器核心
    /// </summary>
    /// <param name="expectedElements">预期元素数量</param>
    /// <param name="falsePositiveRate">误判率</param>
    public BloomFilterCore(long expectedElements, double falsePositiveRate)
    {
        _expectedElements = expectedElements;
        _falsePositiveRate = falsePositiveRate;
        
        // 计算最优的 bitmap 大小和哈希函数数量
        _bitmapSize = CalculateOptimalBitmapSize(expectedElements, falsePositiveRate);
        _hashFunctionCount = CalculateOptimalHashFunctionCount(_bitmapSize, expectedElements);
    }

    /// <summary>
    /// 获取哈希位置
    /// </summary>
    public IEnumerable<long> GetHashPositions(string key)
    {
        var hash1 = MurmurHash(key, 0);
        var hash2 = MurmurHash(key, hash1);

        for (int i = 0; i < _hashFunctionCount; i++)
        {
            yield return Math.Abs((hash1 + i * hash2) % _bitmapSize);
        }
    }

    /// <summary>
    /// 计算最优的 bitmap 大小
    /// </summary>
    private static long CalculateOptimalBitmapSize(long expectedElements, double falsePositiveRate)
    {
        // m = -(n * ln(p)) / (ln(2)^2)
        return (long)Math.Ceiling(-(expectedElements * Math.Log(falsePositiveRate)) / (Math.Log(2) * Math.Log(2)));
    }

    /// <summary>
    /// 计算最优的哈希函数数量
    /// </summary>
    private static int CalculateOptimalHashFunctionCount(long bitmapSize, long expectedElements)
    {
        // k = (m / n) * ln(2)
        return (int)Math.Round((double)bitmapSize / expectedElements * Math.Log(2));
    }

    /// <summary>
    /// MurmurHash 实现
    /// </summary>
    private static long MurmurHash(string key, long seed)
    {
        const ulong m = 0xc6a4a7935bd1e995UL;
        const int r = 47;

        var data = Encoding.UTF8.GetBytes(key);
        ulong h = (ulong)(seed ^ data.Length);
        int len = data.Length;
        int i = 0;

        while (len >= 8)
        {
            ulong k = BitConverter.ToUInt64(data, i);
            k *= m;
            k ^= k >> r;
            k *= m;
            h ^= k;
            h *= m;
            i += 8;
            len -= 8;
        }

        switch (len)
        {
            case 7: h ^= (ulong)data[i + 6] << 48; goto case 6;
            case 6: h ^= (ulong)data[i + 5] << 40; goto case 5;
            case 5: h ^= (ulong)data[i + 4] << 32; goto case 4;
            case 4: h ^= (ulong)data[i + 3] << 24; goto case 3;
            case 3: h ^= (ulong)data[i + 2] << 16; goto case 2;
            case 2: h ^= (ulong)data[i + 1] << 8; goto case 1;
            case 1: h ^= data[i];
                h *= m;
                break;
        }

        h ^= h >> r;
        h *= m;
        h ^= h >> r;

        return (long)h;
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public Abstraction.Interfaces.BloomFilterStats GetStats(long setBitsCount)
    {
        return new Abstraction.Interfaces.BloomFilterStats
        {
            ExpectedElements = _expectedElements,
            BitmapSize = _bitmapSize,
            HashFunctionCount = _hashFunctionCount,
            FalsePositiveRate = _falsePositiveRate,
            ApproximateCount = (long)(-_bitmapSize * Math.Log(1 - (double)setBitsCount / _bitmapSize) / _hashFunctionCount),
            SetBitsCount = setBitsCount
        };
    }
}