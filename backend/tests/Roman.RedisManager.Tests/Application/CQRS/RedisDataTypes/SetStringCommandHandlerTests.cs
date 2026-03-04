using Roman.RedisManager.Application.CQRS.RedisDataTypes;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes
{
    public class SetStringCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ConditionNone_ReturnsSuccess()
        {
            var stub = new StubStringRepository(setResult: true);
            var command = new SetStringCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "test:key",
                Value = "hello",
                Condition = SetCondition.None
            };

            var result = await SetStringCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.Success.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_NotExistsConditionOnExistingKey_ReturnsFailure()
        {
            var stub = new StubStringRepository(setResult: false);
            var command = new SetStringCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "exists:key",
                Value = "new value",
                Condition = SetCondition.NotExists
            };

            var result = await SetStringCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.Success.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubStringRepository(setResult: true);

            await Should.ThrowAsync<ArgumentNullException>(
                () => SetStringCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new SetStringCommand { GroupId = Guid.NewGuid(), Key = "k", Value = "v" };

            await Should.ThrowAsync<ArgumentNullException>(
                () => SetStringCommandHandler.Handle(command, null!));
        }

        [Fact]
        public async Task Handle_ValidCommand_ForwardsConditionAndTtlToRepository()
        {
            var capturing = new CapturingStringRepository();
            var command = new SetStringCommand
            {
                GroupId = Guid.NewGuid(),
                Key = "k",
                Value = "v",
                Ttl = TimeSpan.FromMinutes(10),
                Condition = SetCondition.NotExists
            };

            await SetStringCommandHandler.Handle(command, capturing);

            capturing.ReceivedCondition.ShouldBe(SetCondition.NotExists);
            capturing.ReceivedTtl.ShouldBe(TimeSpan.FromMinutes(10));
        }

        private sealed class StubStringRepository : IRedisStringRepository
        {
            private readonly bool _setResult;

            public StubStringRepository(bool setResult) => _setResult = setResult;

            public Task<bool> StringSetAsync(Guid groupId, string key, string value, TimeSpan? ttl, SetCondition condition) =>
                Task.FromResult(_setResult);

            public Task<string?> StringGetAsync(Guid groupId, string key) =>
                Task.FromResult<string?>(null);
        }

        private sealed class CapturingStringRepository : IRedisStringRepository
        {
            public SetCondition ReceivedCondition { get; private set; }
            public TimeSpan? ReceivedTtl { get; private set; }

            public Task<bool> StringSetAsync(Guid groupId, string key, string value, TimeSpan? ttl, SetCondition condition)
            {
                ReceivedCondition = condition;
                ReceivedTtl = ttl;
                return Task.FromResult(true);
            }

            public Task<string?> StringGetAsync(Guid groupId, string key) =>
                Task.FromResult<string?>(null);
        }
    }
}
