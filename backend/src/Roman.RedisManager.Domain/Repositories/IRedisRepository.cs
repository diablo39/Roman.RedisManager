using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisRepository
    {
        /// <summary>
        /// Searches for keys in the specified server group. The cursor value is
        /// passed as a string so that callers can encode a composite state when
        /// scanning multiple cluster nodes. The returned <see cref="RedisSearchResult"/>
        /// may include <see cref="RedisSearchResult.NodeCursors"/> when operating
        /// against a cluster; for standalone groups the caller can ignore this.
        /// </summary>
        Task<RedisSearchResult> SearchForKeysAsync(
            Guid groupId,
            string pattern,
            string cursor,
            int pageSize);

        Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId);

        Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port);
    }
}
