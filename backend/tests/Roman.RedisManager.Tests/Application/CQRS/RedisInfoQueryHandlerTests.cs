using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class RedisInfoQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithValidInfo_ReturnsDto()
        {
            var sample = new RedisInfo(new Dictionary<string, IReadOnlyDictionary<string, string>>
            {
                ["Server"] = new Dictionary<string, string>
                {
                    ["redis_version"] = "7.2.12",
                    ["tcp_port"] = "8000"
                },
                ["Clients"] = new Dictionary<string, string>
                {
                    ["connected_clients"] = "1"
                }
            });

            var repository = new StubRedisRepository(sample);
            var query = new RedisInfoQuery { GroupId = Guid.NewGuid(), Host = "localhost", Port = 6379 };

            var result = await RedisInfoQueryHandler.Handle(query, repository);

            result.Sections.ShouldNotBeNull();
            result.Sections.Count.ShouldBe(2);
            result.Sections["Server"]["redis_version"].ShouldBe("7.2.12");
        }

        [Fact]
        public async Task Handle_MissingGroup_PropagatesException()
        {
            var repository = new StubRedisRepository(null);
            var query = new RedisInfoQuery { GroupId = Guid.NewGuid(), Host = "localhost", Port = 6379 };

            await Should.ThrowAsync<KeyNotFoundException>(() => RedisInfoQueryHandler.Handle(query, repository));
        }

        private sealed class StubRedisRepository : IRedisRepository
        {
            private readonly RedisInfo? _info;
            public StubRedisRepository(RedisInfo? info) => _info = info;

            public Task<RedisSearchResult> SearchForKeysAsync(Guid groupId, string predicate) =>
                Task.FromResult(new RedisSearchResult(Array.Empty<RedisKey>(), 0));

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                Task.FromResult<IReadOnlyCollection<RedisServerNode>>(Array.Empty<RedisServerNode>());

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port)
            {
                if (_info is null)
                    throw new KeyNotFoundException("no info");

                return Task.FromResult(_info);
            }
        }
    }
}
