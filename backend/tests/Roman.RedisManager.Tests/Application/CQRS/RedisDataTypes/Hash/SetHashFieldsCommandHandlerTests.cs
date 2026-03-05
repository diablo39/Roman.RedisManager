using Roman.RedisManager.Application.CQRS.RedisDataTypes.Hash;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.Hash
{
    public class SetHashFieldsCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var stub = new StubHashRepository();
            var command = new SetHashFieldsCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:hash",
                Fields = new Dictionary<string, string> { ["name"] = "Alice", ["age"] = "30" }
            };

            var result = await SetHashFieldsCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_WithTtl_ReturnsSuccess()
        {
            var stub = new StubHashRepository();
            var command = new SetHashFieldsCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:hash:ttl",
                Fields = new Dictionary<string, string> { ["key"] = "value" },
                Ttl = TimeSpan.FromMinutes(5)
            };

            var result = await SetHashFieldsCommandHandler.Handle(command, stub);

            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubHashRepository();

            await Should.ThrowAsync<ArgumentNullException>(
                () => SetHashFieldsCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new SetHashFieldsCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "k",
                Fields = new Dictionary<string, string> { ["f"] = "v" }
            };

            await Should.ThrowAsync<ArgumentNullException>(
                () => SetHashFieldsCommandHandler.Handle(command, null!));
        }

        private sealed class StubHashRepository : IRedisHashRepository
        {
            public Task SetHashFieldsAsync(Guid groupId, string key, IReadOnlyDictionary<string, string> fields, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<RedisScanResult<RedisHashEntry>> HashScanAsync(Guid groupId, string key, long cursor, int pageSize) =>
                Task.FromResult(new RedisScanResult<RedisHashEntry>(0, Array.Empty<RedisHashEntry>()));

            public Task<long> RemoveHashFieldsAsync(Guid groupId, string key, IReadOnlyCollection<string> fields) =>
                Task.FromResult(0L);
        }
    }
}
