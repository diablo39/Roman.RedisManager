using Roman.RedisManager.Application.CQRS.Data;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS.Data
{
    public class RemoveHashFieldsCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsRemovedCount()
        {
            var stub = new StubHashRepository(removedCount: 2);
            var command = new RemoveHashFieldsCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:hash",
                Fields = ["name", "age"]
            };

            var result = await RemoveHashFieldsCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.RemovedCount.ShouldBe(2L);
        }

        [Fact]
        public async Task Handle_FieldsNotFound_ReturnsZero()
        {
            var stub = new StubHashRepository(removedCount: 0);
            var command = new RemoveHashFieldsCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:hash",
                Fields = ["missing"]
            };

            var result = await RemoveHashFieldsCommandHandler.Handle(command, stub);

            result.RemovedCount.ShouldBe(0L);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubHashRepository(0);

            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveHashFieldsCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new RemoveHashFieldsCommand { GroupId = Guid.NewGuid(), Key = "k", Fields = ["f"] };

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
