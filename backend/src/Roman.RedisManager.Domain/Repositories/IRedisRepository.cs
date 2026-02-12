using Roman.RedisManager.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisRepository
    {
        Task<RedisSearchResult> SearchForKeysAsync(string groupId, string predicate);

        Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(string groupId);
    }
}
