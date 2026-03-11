using Roman.RedisManager.Application.CQRS.RedisDataTypes.Hash;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories.RedisDataTypes;

namespace Roman.RedisManager.Tests.Application.CQRS.RedisDataTypes.Hash
{
    public class GetHashFieldsQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ValidQuery_ReturnsFields()
        {

            // Arrange
            var hashEntries = new[]
            {
                new RedisHashEntry("name", "Alice"),
                new RedisHashEntry("age", "30")
            };
            var stub = new StubHashRepository(new RedisScanResult<RedisHashEntry>(0, hashEntries));
            var query = new GetHashFieldsQuery
            {
                GroupId = Guid.NewGuid(),
                Key = "test:hash",
                Cursor = 0,
                PageSize = 100
            };

            // Act
            var result = await GetHashFieldsQueryHandler.Handle(query, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Fields.ShouldNotBeEmpty();
            result.Fields.Count.ShouldBe(2);
            result.Fields.ShouldContain(f => f.Field == "name" && f.Value == "Alice");
            result.Fields.ShouldContain(f => f.Field == "age" && f.Value == "30");
            result.Cursor.ShouldBe(0L);
            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_WithMoreResults_ReturnsNonZeroCursor()
        {

            // Arrange
            var stub = new StubHashRepository(new RedisScanResult<RedisHashEntry>(99, new[] { new RedisHashEntry("f", "v") }));
            var query = new GetHashFieldsQuery { GroupId = Guid.NewGuid(), Key = "test:hash", PageSize = 1 };

            // Act
            var result = await GetHashFieldsQueryHandler.Handle(query, stub);

            // Assert
            result.Cursor.ShouldBe(99L);
            result.HasMoreResults.ShouldBeTrue();
            result.Fields.Count.ShouldBe(1);
            result.Fields.ShouldContain(f => f.Field == "f" && f.Value == "v");
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubHashRepository(new RedisScanResult<RedisHashEntry>(0, Array.Empty<RedisHashEntry>()));

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => GetHashFieldsQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var query = new GetHashFieldsQuery { GroupId = Guid.NewGuid(), Key = "k" };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => GetHashFieldsQueryHandler.Handle(query, null!));
        }

        private sealed class StubHashRepository : IRedisHashRepository
        {
            private readonly RedisScanResult<RedisHashEntry> _scanResult;

            public StubHashRepository(RedisScanResult<RedisHashEntry> scanResult) => _scanResult = scanResult;

            public Task SetHashFieldsAsync(Guid groupId, string key, IReadOnlyDictionary<string, string> fields, TimeSpan? ttl) =>
                Task.CompletedTask;

            public Task<RedisScanResult<RedisHashEntry>> HashScanAsync(Guid groupId, string key, long cursor, int pageSize) =>
                Task.FromResult(_scanResult);

            public Task<long> RemoveHashFieldsAsync(Guid groupId, string key, IReadOnlyCollection<string> fields) =>
                Task.FromResult(0L);
        }
    }
}
