using Roman.RedisManager.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisRepository
    {
        Task<RedisSearchResult> SearchForKeysAsync(Guid groupId, string predicate);

        Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId);
    }
}
