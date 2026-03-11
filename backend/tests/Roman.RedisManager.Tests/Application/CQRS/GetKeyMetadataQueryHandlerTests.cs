using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class GetKeyMetadataQueryHandlerTests
    {
        [Fact]
        public async Task Handle_StringKeyWithoutTtl_ReturnsStringTypeAndNullTtl()
        {

            // Arrange
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.String, null));
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "test:key" };

            // Act
            var result = await GetKeyMetadataQueryHandler.Handle(query, stub);

            // Assert
            result.ShouldNotBeNull();
            result.Metadata.Type.ShouldBe("String");
            result.Metadata.TtlMilliseconds.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_KeyWithTtl_ReturnsTtlMilliseconds()
        {

            // Arrange
            var ttl = TimeSpan.FromSeconds(30);
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.String, ttl));
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "test:key" };

            // Act
            var result = await GetKeyMetadataQueryHandler.Handle(query, stub);

            // Assert
            result.Metadata.TtlMilliseconds.ShouldBe((long)ttl.TotalMilliseconds);
            result.Metadata.Type.ShouldBe("String");
        }

        [Fact]
        public async Task Handle_ListKey_ReturnsListType()
        {

            // Arrange
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.List, null));
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "test:list" };

            // Act
            var result = await GetKeyMetadataQueryHandler.Handle(query, stub);

            // Assert
            result.Metadata.Type.ShouldBe("List");
            result.Metadata.TtlMilliseconds.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.None, null));

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => GetKeyMetadataQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "k" };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => GetKeyMetadataQueryHandler.Handle(query, null!));
        }

        private sealed class StubKeyRepository : IRedisKeyRepository
        {
            private readonly RedisKeyMetadata _metadata;

            public StubKeyRepository(RedisKeyMetadata metadata) => _metadata = metadata;

            public Task<bool> DeleteKeyAsync(Guid groupId, string key) =>
                Task.FromResult(false);

            public Task<RedisKeyMetadata> GetKeyMetadataAsync(Guid groupId, string key) =>
                Task.FromResult(_metadata);

            public Task<RedisKeyValue> GetKeyValueAsync(Guid groupId, string key) =>
                Task.FromResult(new RedisKeyValue(RedisDataType.None));
        }
    }
}
