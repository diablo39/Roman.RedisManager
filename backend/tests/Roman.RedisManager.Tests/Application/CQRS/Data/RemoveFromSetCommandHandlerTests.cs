using Roman.RedisManager.Application.CQRS.Data;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS.Data
{
    public class RemoveFromSetCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsRemovedCount()
        {
            var stub = new StubSetRepository(removedCount: 2);
            var command = new RemoveFromSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:set",
                Members = ["y", "z"]
            };

            var result = await RemoveFromSetCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.RemovedCount.ShouldBe(2L);
        }

        [Fact]
        public async Task Handle_MembersNotFound_ReturnsZero()
        {
            var stub = new StubSetRepository(removedCount: 0);
            var command = new RemoveFromSetCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:set",
                Members = ["missing"]
            };

            var result = await RemoveFromSetCommandHandler.Handle(command, stub);

            result.RemovedCount.ShouldBe(0L);
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubSetRepository(0);

            await Should.ThrowAsync<ArgumentNullException>(
                () => RemoveFromSetCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new RemoveFromSetCommand { GroupId = Guid.NewGuid(), Key = "k", Members = ["m"] };

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
