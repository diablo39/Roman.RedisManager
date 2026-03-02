using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.Server;
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
            result.Sections["Server"]["tcp_port"].ShouldBe("8000");
            result.Sections["Clients"]["connected_clients"].ShouldBe("1");
        }

        [Fact]
        public async Task Handle_MissingGroup_PropagatesException()
        {
            var repository = new StubRedisRepository(null);
            var query = new RedisInfoQuery { GroupId = Guid.NewGuid(), Host = "localhost", Port = 6379 };

            await Should.ThrowAsync<KeyNotFoundException>(() => RedisInfoQueryHandler.Handle(query, repository));
        }

        [Fact]
        public async Task Handle_EmptySections_ReturnsEmptySections()
        {
            var emptyInfo = new RedisInfo(new Dictionary<string, IReadOnlyDictionary<string, string>>());
            var repository = new StubRedisRepository(emptyInfo);
            var query = new RedisInfoQuery { GroupId = Guid.NewGuid(), Host = "localhost", Port = 6379 };

            var result = await RedisInfoQueryHandler.Handle(query, repository);

            result.Sections.ShouldNotBeNull();
            result.Sections.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var repository = new StubRedisRepository(
                new RedisInfo(new Dictionary<string, IReadOnlyDictionary<string, string>>()));

            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisInfoQueryHandler.Handle(null!, repository));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var query = new RedisInfoQuery { GroupId = Guid.NewGuid(), Host = "localhost", Port = 6379 };

            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisInfoQueryHandler.Handle(query, null!));
        }

        private sealed class StubRedisRepository : IRedisRepository
        {
            private readonly RedisInfo? _info;
            public StubRedisRepository(RedisInfo? info) => _info = info;

            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string cursor, int pageSize) =>
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
