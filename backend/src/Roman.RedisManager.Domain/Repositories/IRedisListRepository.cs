using Roman.RedisManager.Domain.Entities.RedisData;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisListRepository
    {
        Task ListPushAsync(Guid groupId, string key, IReadOnlyCollection<string> values, ListDirection direction, TimeSpan? ttl);

        Task<IReadOnlyCollection<string>> ListRangeAsync(Guid groupId, string key, long start, long stop);

        Task<long> ListRemoveAsync(Guid groupId, string key, string value, long count);
    }
}
