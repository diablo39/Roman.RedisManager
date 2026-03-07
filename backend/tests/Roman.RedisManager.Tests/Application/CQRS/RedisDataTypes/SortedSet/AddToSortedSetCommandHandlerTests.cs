using Roman.RedisManager.Application.CQRS.RedisDataTypes.SortedSet;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.SortedSet
{
    public class AddToSortedSetCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {

            // Arrange
            var stub = new StubSortedSetRepository();
            var command = new AddToSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:zset",
                Entries =
                [
                    new RedisSortedSetEntry("alpha", 1.0),
                    new RedisSortedSetEntry("beta", 2.5)
                ]
            };

            // Act
            var result = await AddToSortedSetCommandHandler.Handle(command, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_WithTtl_ReturnsSuccess()
        {

            // Arrange
            var stub = new StubSortedSetRepository();
            var command = new AddToSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:zset:ttl",
                Entries = [new RedisSortedSetEntry("a", 1.0)],
                Ttl = TimeSpan.FromMinutes(5)
            };

            // Act
            var result = await AddToSortedSetCommandHandler.Handle(command, stub);

            // Assert
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubSortedSetRepository();

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => AddToSortedSetCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var command = new AddToSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "k",
                Entries = [new RedisSortedSetEntry("m", 1.0)]
            };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => AddToSortedSetCommandHandler.Handle(command, null!));
        }

        [Fact]
        public async Task Handle_ValidCommand_ForwardsEntriesAndTtlToRepository()
        {

            // Arrange
            var capturing = new CapturingSortedSetRepository();
            var entries = new[] { new RedisSortedSetEntry("alpha", 1.0), new RedisSortedSetEntry("beta", 2.5) };
            var ttl = TimeSpan.FromHours(1);
            var command = new AddToSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:zset",
                Entries = entries,
                Ttl = ttl
            };

            // Act
            await AddToSortedSetCommandHandler.Handle(command, capturing);

            // Assert
            capturing.ReceivedEntries.ShouldBe(entries);
            capturing.ReceivedTtl.ShouldBe(ttl);
        }

        private sealed class StubSortedSetRepository : IRedisSortedSetRepository
        {
            public Task AddToSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<RedisSortedSetEntry> entries, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<IReadOnlyCollection<RedisSortedSetEntry>> GetSortedSetRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult<IReadOnlyCollection<RedisSortedSetEntry>>(Array.Empty<RedisSortedSetEntry>());

            public Task<long> RemoveFromSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<string> members) =>
                Task.FromResult(0L);
        }

        private sealed class CapturingSortedSetRepository : IRedisSortedSetRepository
        {
            public IReadOnlyCollection<RedisSortedSetEntry>? ReceivedEntries { get; private set; }
            public TimeSpan? ReceivedTtl { get; private set; }

            public Task AddToSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<RedisSortedSetEntry> entries, TimeSpan? ttl)
            {
                ReceivedEntries = entries;
                ReceivedTtl = ttl;
                return Task.CompletedTask;
            }

            public Task<IReadOnlyCollection<RedisSortedSetEntry>> GetSortedSetRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult<IReadOnlyCollection<RedisSortedSetEntry>>(Array.Empty<RedisSortedSetEntry>());

            public Task<long> RemoveFromSortedSetAsync(Guid groupId, string key, IReadOnlyCollection<string> members) =>
                Task.FromResult(0L);
        }
    }
}
