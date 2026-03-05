using Roman.RedisManager.Application.CQRS.RedisDataTypes.Set;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.Set
{
    public class GetSetMembersQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ValidQuery_ReturnsMembers()
        {
            var expectedMembers = new[] { "x", "y", "z" };
            var scanResult = new RedisScanResult<string>(0, expectedMembers);
            var stub = new StubSetRepository(scanResult);
            var query = new GetSetMembersQuery
            {
                GroupId = Guid.NewGuid(),
                Key = "test:set",
                Cursor = 0,
                PageSize = 100
            };

            var result = await GetSetMembersQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Members.ShouldNotBeEmpty();
            result.Members.ShouldBe(expectedMembers);
            result.Cursor.ShouldBe(0L);
            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_WithMoreResults_ReturnsNonZeroCursor()
        {
            var scanResult = new RedisScanResult<string>(42, new[] { "a", "b" });
            var stub = new StubSetRepository(scanResult);
            var query = new GetSetMembersQuery
            {
                GroupId = Guid.NewGuid(),
                Key = "test:large:set",
                PageSize = 2
            };

            var result = await GetSetMembersQueryHandler.Handle(query, stub);

            result.Cursor.ShouldBe(42L);
            result.HasMoreResults.ShouldBeTrue();
            result.Members.Count().ShouldBe(2);
            result.Members.ShouldBe(new[] { "a", "b" });
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var stub = new StubSetRepository(new RedisScanResult<string>(0, Array.Empty<string>()));

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetSetMembersQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var query = new GetSetMembersQuery { GroupId = Guid.NewGuid(), Key = "k" };

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetSetMembersQueryHandler.Handle(query, null!));
        }

        private sealed class StubSetRepository : IRedisSetRepository
        {
            private readonly RedisScanResult<string> _scanResult;

            public StubSetRepository(RedisScanResult<string> scanResult) => _scanResult = scanResult;

            public Task SetAddAsync(Guid groupId, string key, IReadOnlyCollection<string> members, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<RedisScanResult<string>> SetScanAsync(Guid groupId, string key, long cursor, int pageSize) =>
                Task.FromResult(_scanResult);

            public Task<long> SetRemoveAsync(Guid groupId, string key, IReadOnlyCollection<string> members) =>
                Task.FromResult(0L);
        }
    }
}
