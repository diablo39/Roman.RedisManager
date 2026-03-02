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
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.String, null));
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "test:key" };

            var result = await GetKeyMetadataQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Metadata.Type.ShouldBe("String");
            result.Metadata.TtlMilliseconds.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_KeyWithTtl_ReturnsTtlMilliseconds()
        {
            var ttl = TimeSpan.FromSeconds(30);
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.String, ttl));
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "test:key" };

            var result = await GetKeyMetadataQueryHandler.Handle(query, stub);

            result.Metadata.TtlMilliseconds.ShouldBe((long)ttl.TotalMilliseconds);
        }

        [Fact]
        public async Task Handle_ListKey_ReturnsListType()
        {
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.List, null));
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "test:list" };

            var result = await GetKeyMetadataQueryHandler.Handle(query, stub);

            result.Metadata.Type.ShouldBe("List");
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var stub = new StubKeyRepository(new RedisKeyMetadata(RedisDataType.None, null));

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetKeyMetadataQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var query = new GetKeyMetadataQuery { GroupId = Guid.NewGuid(), Key = "k" };

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
