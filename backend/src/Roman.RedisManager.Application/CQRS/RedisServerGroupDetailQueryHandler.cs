using Roman.RedisManager.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Web.Wolverine
{
    public class RedisServerGroupDetailQuery
    {
        public string Id { get; set; } = string.Empty;
    }

    public record RedisServerNodeDto(string Host, int Port, string Role);

    public record RedisServerGroupDetailQueryResult(IReadOnlyCollection<RedisServerNodeDto> Nodes);

    public static class RedisServerGroupDetailQueryHandler
    {
        public static async Task<RedisServerGroupDetailQueryResult> Handle(RedisServerGroupDetailQuery query, IRedisRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);

            ArgumentNullException.ThrowIfNull(repository);

            var nodes = await repository.GetServerNodesAsync(query.Id).ConfigureAwait(false);

            var dto = nodes
                .Select(node => new RedisServerNodeDto(node.Host, node.Port, node.Role))
                .ToList();

            return new RedisServerGroupDetailQueryResult(dto);
        }
    }
}
