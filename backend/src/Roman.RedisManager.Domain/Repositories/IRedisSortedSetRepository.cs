using Roman.RedisManager.Domain.Entities.RedisData;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisSortedSetRepository
    {
        Task AddToSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<RedisSortedSetEntry> entries, TimeSpan? ttl);

        Task<IReadOnlyCollection<RedisSortedSetEntry>> GetSortedSetRangeAsync(Guid groupId, string key, long start, long stop);

        Task<long> RemoveFromSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<string> members);
    }
}
