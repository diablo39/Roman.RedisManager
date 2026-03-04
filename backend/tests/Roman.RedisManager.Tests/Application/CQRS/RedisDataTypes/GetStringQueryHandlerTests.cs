using Roman.RedisManager.Application.CQRS.RedisDataTypes;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes
{
    public class GetStringQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingKey_ReturnsValue()
        {
            var stub = new StubStringRepository(value: "hello world");
            var query = new GetStringQuery { GroupId = Guid.NewGuid(), Key = "test:key" };

            var result = await GetStringQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Value.ShouldBe("hello world");
        }

        [Fact]
        public async Task Handle_NonExistentKey_ReturnsNull()
        {
            var stub = new StubStringRepository(value: null);
            var query = new GetStringQuery { GroupId = Guid.NewGuid(), Key = "missing:key" };

            var result = await GetStringQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Value.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var stub = new StubStringRepository(value: null);

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetStringQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var query = new GetStringQuery { GroupId = Guid.NewGuid(), Key = "k" };

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetStringQueryHandler.Handle(query, null!));
        }

        private sealed class StubStringRepository : IRedisStringRepository
        {
            private readonly string? _value;

            public StubStringRepository(string? value) => _value = value;

            public Task<bool> StringSetAsync(Guid groupId, string key, string value, TimeSpan? ttl, SetCondition condition) =>
                Task.FromResult(true);

            public Task<string?> StringGetAsync(Guid groupId, string key) =>
                Task.FromResult(_value);
        }
    }
}
