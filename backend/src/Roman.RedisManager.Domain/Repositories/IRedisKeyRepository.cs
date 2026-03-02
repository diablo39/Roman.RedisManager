using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisKeyRepository
    {
        Task<bool> DeleteKeyAsync(Guid groupId, string key);

        Task<RedisKeyMetadata> GetKeyMetadataAsync(Guid groupId, string key);

        Task<RedisKeyValue> GetKeyValueAsync(Guid groupId, string key);
    }
}
