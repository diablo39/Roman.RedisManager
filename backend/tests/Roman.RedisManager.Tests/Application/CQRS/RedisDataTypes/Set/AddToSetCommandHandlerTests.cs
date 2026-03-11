using Roman.RedisManager.Application.CQRS.RedisDataTypes.Set;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.Set
{
    public class AddToSetCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {

            // Arrange
            var stub = new StubSetRepository();
            var command = new AddToSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:set",
                Members = ["x", "y", "z"]
            };

            // Act
            var result = await AddToSetCommandHandler.Handle(command, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_WithTtl_ReturnsSuccess()
        {

            // Arrange
            var stub = new StubSetRepository();
            var command = new AddToSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:set:ttl",
                Members = ["a"],
                Ttl = TimeSpan.FromMinutes(10)
            };

            // Act
            var result = await AddToSetCommandHandler.Handle(command, stub);

            // Assert
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubSetRepository();

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => AddToSetCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var command = new AddToSetCommand { GroupId = Guid.NewGuid(), Key = "k", Members = ["m"] };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => AddToSetCommandHandler.Handle(command, null!));
        }

        private sealed class StubSetRepository : IRedisSetRepository
        {
            public Task SetAddAsync(Guid groupId, string key, IReadOnlyCollection<string> members, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<RedisScanResult<string>> SetScanAsync(Guid groupId, string key, long cursor, int pageSize) =>
                Task.FromResult(new RedisScanResult<string>(0, Array.Empty<string>()));

            public Task<long> SetRemoveAsync(Guid groupId, string key, IReadOnlyCollection<string> members) =>
                Task.FromResult(0L);
        }
    }
}
