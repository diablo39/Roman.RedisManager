using Roman.RedisManager.Application.CQRS.RedisDataTypes.Set;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.Set
{
    public class RemoveFromSetCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsRemovedCount()
        {

            // Arrange
            var stub = new StubSetRepository(removedCount: 2);
            var command = new RemoveFromSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:set",
                Members = ["y", "z"]
            };

            // Act
            var result = await RemoveFromSetCommandHandler.Handle(command, stub);

            // Assert
            result.ShouldNotBeNull();
            result.RemovedCount.ShouldBe(2L);
        }

        [Fact]
        public async Task Handle_MembersNotFound_ReturnsZero()
        {

            // Arrange
            var stub = new StubSetRepository(removedCount: 0);
            var command = new RemoveFromSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:set",
                Members = ["missing"]
            };

            // Act
            var result = await RemoveFromSetCommandHandler.Handle(command, stub);

            // Assert
            result.RemovedCount.ShouldBe(0L);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubSetRepository(0);

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromSetCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var command = new RemoveFromSetCommand { GroupId = Guid.NewGuid(), Key = "k", Members = ["m"] };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromSetCommandHandler.Handle(command, null!));
        }

        private sealed class StubSetRepository : IRedisSetRepository
        {
            private readonly long _removedCount;

            public StubSetRepository(long removedCount) => _removedCount = removedCount;

            public Task SetAddAsync(Guid groupId, string key, IReadOnlyCollection<string> members, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<RedisScanResult<string>> SetScanAsync(Guid groupId, string key, long cursor, int pageSize) =>
                Task.FromResult(new RedisScanResult<string>(0, Array.Empty<string>()));

            public Task<long> SetRemoveAsync(Guid groupId, string key, IReadOnlyCollection<string> members) =>
                Task.FromResult(_removedCount);
        }
    }
}
