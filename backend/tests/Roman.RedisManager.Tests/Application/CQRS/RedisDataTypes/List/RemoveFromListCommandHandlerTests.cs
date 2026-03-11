using Roman.RedisManager.Application.CQRS.RedisDataTypes.List;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.List
{
    public class RemoveFromListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsRemovedCount()
        {

            // Arrange
            var stub = new StubListRepository(removedCount: 2);
            var command = new RemoveFromListCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Value = "b",
                Count = 2
            };

            // Act
            var result = await RemoveFromListCommandHandler.Handle(command, stub);

            // Assert
            result.ShouldNotBeNull();
            result.RemovedCount.ShouldBe(2L);
        }

        [Fact]
        public async Task Handle_ValueNotFound_ReturnsZero()
        {

            // Arrange
            var stub = new StubListRepository(removedCount: 0);
            var command = new RemoveFromListCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Value = "missing"
            };

            // Act
            var result = await RemoveFromListCommandHandler.Handle(command, stub);

            // Assert
            result.RemovedCount.ShouldBe(0L);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubListRepository(0);

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromListCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var command = new RemoveFromListCommand { GroupId = Guid.NewGuid(), Key = "k", Value = "v" };

            // Act

            // Assert
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
