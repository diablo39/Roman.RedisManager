using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class RedisServerGroupDetailQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingGroup_ReturnsNodes()
        {
            var repository = new InMemoryRedisRepository(new Dictionary<Guid, IReadOnlyCollection<RedisServerNode>>
            {
                [Guid.Parse("11111111-1111-1111-1111-111111111111")] = new List<RedisServerNode>
                {
                    new RedisServerNode("127.0.0.1", 6379, "master"),
                    new RedisServerNode("127.0.0.1", 6380, "slave")
                }
            });

            var query = new RedisServerGroupDetailQuery { Id = Guid.Parse("11111111-1111-1111-1111-111111111111") };

            var result = await RedisServerGroupDetailQueryHandler.Handle(query, repository);

            result.Nodes.ShouldNotBeNull();
            result.Nodes.Count.ShouldBe(2);
            result.Nodes.ShouldContain(node => node.Role == "master");
            result.Nodes.ShouldContain(node => node.Role == "slave");
            result.Nodes.ShouldContain(node => node.Host == "127.0.0.1" && node.Port == 6379 && node.Role == "master");
            result.Nodes.ShouldContain(node => node.Host == "127.0.0.1" && node.Port == 6380 && node.Role == "slave");
        }

        [Fact]
        public async Task Handle_MissingGroup_ThrowsKeyNotFoundException()
        {
            var repository = new InMemoryRedisRepository(new Dictionary<Guid, IReadOnlyCollection<RedisServerNode>>());
            var query = new RedisServerGroupDetailQuery { Id = Guid.Parse("00000000-0000-0000-0000-000000000000") };

            await Should.ThrowAsync<KeyNotFoundException>(() => RedisServerGroupDetailQueryHandler.Handle(query, repository));
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var repository = new InMemoryRedisRepository(new Dictionary<Guid, IReadOnlyCollection<RedisServerNode>>());

            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisServerGroupDetailQueryHandler.Handle(null!, repository));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var query = new RedisServerGroupDetailQuery { Id = Guid.NewGuid() };

            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisServerGroupDetailQueryHandler.Handle(query, null!));
        }

        private sealed class InMemoryRedisRepository : IRedisRepository
        {
            private readonly IReadOnlyDictionary<Guid, IReadOnlyCollection<RedisServerNode>> _nodes;

            public InMemoryRedisRepository(IReadOnlyDictionary<Guid, IReadOnlyCollection<RedisServerNode>> nodes)
            {
                _nodes = nodes;
            }

            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string? continuationToken, int pageSize)
            {
                return Task.FromResult(new RedisSearchResult(Array.Empty<RedisKey>(), 0));
            }

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId)
            {
                if (!_nodes.TryGetValue(groupId, out var nodes))
                {
                    throw new KeyNotFoundException($"Group '{groupId}' was not found.");
                }

                return Task.FromResult(nodes);
            }

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port)
            {
                // not used in these tests
                return Task.FromResult(new RedisInfo(new Dictionary<string, IReadOnlyDictionary<string, string>>()));
            }
        }
    }
}
