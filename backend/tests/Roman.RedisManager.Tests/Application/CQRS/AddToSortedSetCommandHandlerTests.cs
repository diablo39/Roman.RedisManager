using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class AddToSortedSetCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
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

            var result = await AddToSortedSetCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_WithTtl_ReturnsSuccess()
        {
            var stub = new StubSortedSetRepository();
            var command = new AddToSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:zset:ttl",
                Entries = [new RedisSortedSetEntry("a", 1.0)],
                Ttl = TimeSpan.FromMinutes(5)
            };

            var result = await AddToSortedSetCommandHandler.Handle(command, stub);

            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubSortedSetRepository();

            await Should.ThrowAsync<ArgumentNullException>(
                () => AddToSortedSetCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new AddToSortedSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "k",
                Entries = [new RedisSortedSetEntry("m", 1.0)]
            };

            await Should.ThrowAsync<ArgumentNullException>(
                () => AddToSortedSetCommandHandler.Handle(command, null!));
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
    }
}
