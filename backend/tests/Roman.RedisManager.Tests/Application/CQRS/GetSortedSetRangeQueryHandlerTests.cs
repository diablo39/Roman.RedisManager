using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class GetSortedSetRangeQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ValidQuery_ReturnsEntries()
        {
            var expectedEntries = new[]
            {
                new RedisSortedSetEntry("alpha", 1.0),
                new RedisSortedSetEntry("beta", 2.5),
                new RedisSortedSetEntry("gamma", 3.0)
            };
            var stub = new StubSortedSetRepository(expectedEntries);
            var query = new GetSortedSetRangeQuery
            {
                GroupId = Guid.NewGuid(),
                Key = "test:zset",
                Start = 0,
                Stop = -1
            };

            var result = await GetSortedSetRangeQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Entries.ShouldNotBeEmpty();
            result.Entries.Count.ShouldBe(3);
            result.Entries.ShouldContain(e => e.Member == "alpha" && e.Score == 1.0);
            result.Entries.ShouldContain(e => e.Member == "gamma" && e.Score == 3.0);
        }

        [Fact]
        public async Task Handle_EmptySortedSet_ReturnsEmptyCollection()
        {
            var stub = new StubSortedSetRepository(Array.Empty<RedisSortedSetEntry>());
            var query = new GetSortedSetRangeQuery { GroupId = Guid.NewGuid(), Key = "test:empty" };

            var result = await GetSortedSetRangeQueryHandler.Handle(query, stub);

            result.Entries.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var stub = new StubSortedSetRepository(Array.Empty<RedisSortedSetEntry>());

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetSortedSetRangeQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var query = new GetSortedSetRangeQuery { GroupId = Guid.NewGuid(), Key = "k" };

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetSortedSetRangeQueryHandler.Handle(query, null!));
        }

        private sealed class StubSortedSetRepository : IRedisSortedSetRepository
        {
            private readonly IReadOnlyCollection<RedisSortedSetEntry> _rangeResult;

            public StubSortedSetRepository(IReadOnlyCollection<RedisSortedSetEntry> rangeResult) => _rangeResult = rangeResult;

            public Task AddToSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<RedisSortedSetEntry> entries, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<IReadOnlyCollection<RedisSortedSetEntry>> GetSortedSetRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult(_rangeResult);

            public Task<long> RemoveFromSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<string> members) =>
                Task.FromResult(0L);
        }
    }
}
