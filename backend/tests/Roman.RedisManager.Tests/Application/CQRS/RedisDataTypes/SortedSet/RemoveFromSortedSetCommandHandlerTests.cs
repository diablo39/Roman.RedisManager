using Roman.RedisManager.Application.CQRS.RedisDataTypes.SortedSet;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.SortedSet
{
    public class RemoveFromSortedSetCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsRemovedCount()
        {
            var stub = new StubSortedSetRepository(removedCount: 2);
            var command = new RemoveFromSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:zset",
                Members = ["alpha", "beta"]
            };

            var result = await RemoveFromSortedSetCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.RemovedCount.ShouldBe(2L);
        }

        [Fact]
        public async Task Handle_MembersNotFound_ReturnsZero()
        {
            var stub = new StubSortedSetRepository(removedCount: 0);
            var command = new RemoveFromSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:zset",
                Members = ["missing"]
            };

            var result = await RemoveFromSortedSetCommandHandler.Handle(command, stub);

            result.RemovedCount.ShouldBe(0L);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubSortedSetRepository(0);

            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromSortedSetCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new RemoveFromSortedSetCommand { GroupId = Guid.NewGuid(), Key = "k", Members = ["m"] };

            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromSortedSetCommandHandler.Handle(command, null!));
        }

        private sealed class StubSortedSetRepository : IRedisSortedSetRepository
        {
            private readonly long _removedCount;

            public StubSortedSetRepository(long removedCount) => _removedCount = removedCount;

            public Task AddToSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<RedisSortedSetEntry> entries, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<IReadOnlyCollection<RedisSortedSetEntry>> GetSortedSetRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult<IReadOnlyCollection<RedisSortedSetEntry>>(Array.Empty<RedisSortedSetEntry>());

            public Task<long> RemoveFromSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<string> members) =>
                Task.FromResult(_removedCount);
        }
    }
}
