using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Web.Wolverine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests
{
    public class RedisServerGroupDetailQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingGroup_ReturnsNodes()
        {
            var repository = new InMemoryRedisRepository(new Dictionary<string, IReadOnlyCollection<RedisServerNode>>
            {
                ["group-a"] = new List<RedisServerNode>
                {
                    new RedisServerNode("127.0.0.1", 6379, "master"),
                    new RedisServerNode("127.0.0.1", 6380, "slave")
                }
            });

            var query = new RedisServerGroupDetailQuery { Id = "group-a" };

            var result = await RedisServerGroupDetailQueryHandler.Handle(query, repository);

            result.Nodes.ShouldNotBeNull();
            result.Nodes.Count.ShouldBe(2);
            result.Nodes.ShouldContain(node => node.Role == "master");
            result.Nodes.ShouldContain(node => node.Role == "slave");
        }

        [Fact]
        public async Task Handle_MissingGroup_ThrowsKeyNotFoundException()
        {
            var repository = new InMemoryRedisRepository(new Dictionary<string, IReadOnlyCollection<RedisServerNode>>());
            var query = new RedisServerGroupDetailQuery { Id = "missing" };

            await Should.ThrowAsync<KeyNotFoundException>(() => RedisServerGroupDetailQueryHandler.Handle(query, repository));
        }

        private sealed class InMemoryRedisRepository : IRedisRepository
        {
            private readonly IReadOnlyDictionary<string, IReadOnlyCollection<RedisServerNode>> _nodes;

            public InMemoryRedisRepository(IReadOnlyDictionary<string, IReadOnlyCollection<RedisServerNode>> nodes)
            {
                _nodes = nodes;
            }

            public Task<RedisSearchResult> SearchForKeysAsync(string groupId, string predicate)
            {
                return Task.FromResult(new RedisSearchResult(Array.Empty<RedisKey>(), 0));
            }

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(string groupId)
            {
                if (!_nodes.TryGetValue(groupId, out var nodes))
                {
                    throw new KeyNotFoundException($"Group '{groupId}' was not found.");
                }

                return Task.FromResult(nodes);
            }
        }
    }
}
