using Roman.RedisManager.Application.CQRS.RedisDataTypes.Hash;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.Hash
{
    public class RemoveHashFieldsCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsRemovedCount()
        {

            // Arrange
            var stub = new StubHashRepository(removedCount: 2);
            var command = new RemoveHashFieldsCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:hash",
                Fields = ["name", "age"]
            };

            // Act
            var result = await RemoveHashFieldsCommandHandler.Handle(command, stub);

            // Assert
            result.ShouldNotBeNull();
            result.RemovedCount.ShouldBe(2L);
        }

        [Fact]
        public async Task Handle_FieldsNotFound_ReturnsZero()
        {

            // Arrange
            var stub = new StubHashRepository(removedCount: 0);
            var command = new RemoveHashFieldsCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:hash",
                Fields = ["missing"]
            };

            // Act
            var result = await RemoveHashFieldsCommandHandler.Handle(command, stub);

            // Assert
            result.RemovedCount.ShouldBe(0L);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubHashRepository(0);

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveHashFieldsCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var command = new RemoveHashFieldsCommand { GroupId = Guid.NewGuid(), Key = "k", Fields = ["f"] };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveHashFieldsCommandHandler.Handle(command, null!));
        }

        private sealed class StubHashRepository : IRedisHashRepository
        {
            private readonly long _removedCount;

            public StubHashRepository(long removedCount) => _removedCount = removedCount;

            public Task SetHashFieldsAsync(Guid groupId, string key, IReadOnlyDictionary<string, string> fields, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<RedisScanResult<RedisHashEntry>> HashScanAsync(Guid groupId, string key, long cursor, int pageSize) =>
                Task.FromResult(new RedisScanResult<RedisHashEntry>(0, Array.Empty<RedisHashEntry>()));

            public Task<long> RemoveHashFieldsAsync(Guid groupId, string key, IReadOnlyCollection<string> fields) =>
                Task.FromResult(_removedCount);
        }
    }
}
