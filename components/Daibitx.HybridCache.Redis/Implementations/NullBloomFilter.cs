using Daibitx.HybridCache.Abstraction.Interfaces;

namespace Daibitx.HybridCache.Redis.Implementations
{
    public class NullBloomFilter : IBloomFilter
    {
        // 给一个固定名字避免 null
        public string Name => "NullBloomFilter";

        // 不做任何操作
        public Task AddAsync(string key, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        // 永远返回“不存在”
        public Task<bool> ContainsAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<IEnumerable<bool>> ContainsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
            => Task.FromResult(keys.Select(_ => false));

        // 不做任何操作
        public Task ClearAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        // 返回空统计数据（你自行定义的类型）
        public Task<BloomFilterStats> GetStatsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new BloomFilterStats());
    }
}
