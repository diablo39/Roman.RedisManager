using Roman.RedisManager.Application.CQRS.RedisDataTypes.List;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.List
{
    public class PushToListCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var stub = new StubListRepository();
            var command = new PushToListCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Values = ["a", "b", "c"],
                Direction = ListDirection.Right
            };

            var result = await PushToListCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_LeftDirection_ReturnsSuccess()
        {
            var stub = new StubListRepository();
            var command = new PushToListCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Values = ["z"],
                Direction = ListDirection.Left,
                Ttl = TimeSpan.FromMinutes(5)
            };

            var result = await PushToListCommandHandler.Handle(command, stub);

            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubListRepository();

            await Should.ThrowAsync<ArgumentNullException>(
                () => PushToListCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new PushToListCommand { GroupId = Guid.NewGuid(), Key = "k", Values = ["v"] };

            await Should.ThrowAsync<ArgumentNullException>(
                () => PushToListCommandHandler.Handle(command, null!));
        }

        [Fact]
        public async Task Handle_ValidCommand_ForwardsDirectionAndTtlToRepository()
        {
            var capturing = new CapturingListRepository();
            var command = new PushToListCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Values = ["x"],
                Direction = ListDirection.Left,
                Ttl = TimeSpan.FromSeconds(30)
            };

            await PushToListCommandHandler.Handle(command, capturing);

            capturing.ReceivedDirection.ShouldBe(ListDirection.Left);
            capturing.ReceivedTtl.ShouldBe(TimeSpan.FromSeconds(30));
        }

        private sealed class StubListRepository : IRedisListRepository
        {
            public Task ListPushAsync(Guid groupId, string key, IReadOnlyCollection<string> values, ListDirection direction, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<IReadOnlyCollection<string>> ListRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());

            public Task<long> ListRemoveAsync(Guid groupId, string key, string value, long count) =>
                Task.FromResult(0L);
        }

        private sealed class CapturingListRepository : IRedisListRepository
        {
            public ListDirection ReceivedDirection { get; private set; }
            public TimeSpan? ReceivedTtl { get; private set; }

            public Task ListPushAsync(Guid groupId, string key, IReadOnlyCollection<string> values, ListDirection direction, TimeSpan? ttl)
            {
                ReceivedDirection = direction;
                ReceivedTtl = ttl;
                return Task.CompletedTask;
            }

            public Task<IReadOnlyCollection<string>> ListRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult<IReadOnlyCollection<string>>(Array.Empty<string>());

            public Task<long> ListRemoveAsync(Guid groupId, string key, string value, long count) =>
                Task.FromResult(0L);
        }
    }
}
