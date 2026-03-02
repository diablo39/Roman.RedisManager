using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class RemoveFromListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsRemovedCount()
        {
            var stub = new StubListRepository(removedCount: 2);
            var command = new RemoveFromListCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Value = "b",
                Count = 2
            };

            var result = await RemoveFromListCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.RemovedCount.ShouldBe(2L);
        }

        [Fact]
        public async Task Handle_ValueNotFound_ReturnsZero()
        {
            var stub = new StubListRepository(removedCount: 0);
            var command = new RemoveFromListCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Value = "missing"
            };

            var result = await RemoveFromListCommandHandler.Handle(command, stub);

            result.RemovedCount.ShouldBe(0L);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubListRepository(0);

            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromListCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new RemoveFromListCommand { GroupId = Guid.NewGuid(), Key = "k", Value = "v" };

            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromListCommandHandler.Handle(command, null!));
        }

        private sealed class StubListRepository : IRedisListRepository
        {
            private readonly long _removedCount;

            public StubListRepository(long removedCount) => _removedCount = removedCount;

            public Task ListPushAsync(Guid groupId, string key, IReadOnlyCollection<string> values, ListDirection direction, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<IReadOnlyCollection<string>> ListRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());

            public Task<long> ListRemoveAsync(Guid groupId, string key, string value, long count) =>
                Task.FromResult(_removedCount);
        }
    }
}
