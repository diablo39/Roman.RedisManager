using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class DeleteKeyCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ExistingKey_ReturnsDeleted()
        {
            var stub = new StubKeyRepository(deleteResult: true);
            var command = new DeleteKeyCommand { GroupId = Guid.NewGuid(), Key = "test:key" };

            var result = await DeleteKeyCommandHandler.Handle(command, stub);

            result.ShouldNotBeNull();
            result.Deleted.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_NonExistentKey_ReturnsNotDeleted()
        {
            var stub = new StubKeyRepository(deleteResult: false);
            var command = new DeleteKeyCommand { GroupId = Guid.NewGuid(), Key = "missing:key" };

            var result = await DeleteKeyCommandHandler.Handle(command, stub);

            result.Deleted.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_NullCommand_ThrowsArgumentNullException()
        {
            var stub = new StubKeyRepository(deleteResult: false);

            await Should.ThrowAsync<ArgumentNullException>(
                () => DeleteKeyCommandHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var command = new DeleteKeyCommand { GroupId = Guid.NewGuid(), Key = "k" };

            await Should.ThrowAsync<ArgumentNullException>(
                () => DeleteKeyCommandHandler.Handle(command, null!));
        }

        private sealed class StubKeyRepository : IRedisKeyRepository
        {
            private readonly bool _deleteResult;

            public StubKeyRepository(bool deleteResult) => _deleteResult = deleteResult;

            public Task<bool> DeleteKeyAsync(Guid groupId, string key) =>
                Task.FromResult(_deleteResult);

            public Task<RedisKeyMetadata> GetKeyMetadataAsync(Guid groupId, string key) =>
                Task.FromResult(new RedisKeyMetadata(RedisDataType.String, null));

            public Task<RedisKeyValue> GetKeyValueAsync(Guid groupId, string key) =>
                Task.FromResult(new RedisKeyValue(RedisDataType.None));
        }
    }
}
