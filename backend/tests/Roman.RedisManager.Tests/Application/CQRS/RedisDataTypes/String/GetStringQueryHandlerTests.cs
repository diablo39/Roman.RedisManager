using Roman.RedisManager.Application.CQRS.RedisDataTypes.String;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.String
{
    public class GetStringQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingKey_ReturnsValue()
        {

            // Arrange
            var stub = new StubStringRepository(value: "hello world");
            var query = new GetStringQuery { GroupId = Guid.NewGuid(), Key = "test:key" };

            // Act
            var result = await GetStringQueryHandler.Handle(query, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Value.ShouldBe("hello world");
        }

        [Fact]
        public async Task Handle_NonExistentKey_ReturnsNull()
        {

            // Arrange
            var stub = new StubStringRepository(value: null);
            var query = new GetStringQuery { GroupId = Guid.NewGuid(), Key = "missing:key" };

            // Act
            var result = await GetStringQueryHandler.Handle(query, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Value.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubStringRepository(value: null);

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => GetStringQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var query = new GetStringQuery { GroupId = Guid.NewGuid(), Key = "k" };

            // Act

            // Assert
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
