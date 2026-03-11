using Roman.RedisManager.Application.CQRS.RedisDataTypes.List;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.List
{
    public class GetListRangeQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ValidQuery_ReturnsValues()
        {

            // Arrange
            var expectedValues = new[] { "a", "b", "c" };
            var stub = new StubListRepository(expectedValues);
            var query = new GetListRangeQuery
            {
                GroupId = Guid.NewGuid(),
                Key = "test:list",
                Start = 0,
                Stop = -1
            };

            // Act
            var result = await GetListRangeQueryHandler.Handle(query, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Values.ShouldNotBeEmpty();
            result.Values.ShouldBe(expectedValues);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsEmptyCollection()
        {

            // Arrange
            var stub = new StubListRepository(Array.Empty<string>());
            var query = new GetListRangeQuery
            {
                GroupId = Guid.NewGuid(),
                Key = "test:empty"
            };

            // Act
            var result = await GetListRangeQueryHandler.Handle(query, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Values.ShouldBeEmpty();
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubListRepository(Array.Empty<string>());

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => GetListRangeQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var query = new GetListRangeQuery { GroupId = Guid.NewGuid(), Key = "k" };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => GetListRangeQueryHandler.Handle(query, null!));
        }

        private sealed class StubListRepository : IRedisListRepository
        {
            private readonly IReadOnlyCollection<string> _rangeResult;

            public StubListRepository(IReadOnlyCollection<string> rangeResult) => _rangeResult = rangeResult;

            public Task ListPushAsync(Guid groupId, string key, IReadOnlyCollection<string> values, ListDirection direction, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<IReadOnlyCollection<string>> ListRangeAsync(Guid groupId, string key, long start, long stop) =>
                Task.FromResult(_rangeResult);

            public Task<long> ListRemoveAsync(Guid groupId, string key, string value, long count) =>
                Task.FromResult(0L);
        }
    }
}
