using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisRepository
    {
        Task<RedisSearchResult> SearchForKeysAsync(
            Guid groupId,
            string pattern,
            string? continuationToken,
            int pageSize);

        Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId);

        Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port);
    }
}
