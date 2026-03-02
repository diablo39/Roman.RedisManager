using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class RedisKeysSearchQueryHandlerTests
    {
        [Fact]
        public async Task Handle_WithKeys_ReturnsMappedDtos()
        {
            var stub = new StubRedisRepository(
                new RedisSearchResult(
                    new[] { new RedisKey("key:1"), new RedisKey("key:2") },
                    cursor: 42));

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "key:*",
                Cursor = "0",
                PageSize = 10
            };

            var result = await RedisKeysSearchQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Keys.Count.ShouldBe(2);
            result.Keys.ShouldContain(k => k.Key == "key:1");
            result.Keys.ShouldContain(k => k.Key == "key:2");
            result.Cursor.ShouldBe(42);
            result.HasMoreResults.ShouldBeTrue();
        }

        [Fact]
        public async Task Handle_WithZeroCursorReturned_HasMoreResultsIsFalse()
        {
            var stub = new StubRedisRepository(
                new RedisSearchResult(new[] { new RedisKey("only:key") }, cursor: 0));

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "*",
                Cursor = "0",
                PageSize = 100
            };

            var result = await RedisKeysSearchQueryHandler.Handle(query, stub);

            result.HasMoreResults.ShouldBeFalse();
        }

        [Fact]
        public async Task Handle_PreservesNodeCursorsFromRepository()
        {
            var nodemap = new Dictionary<string,long> { ["1:6379"] = 5 };
            var stub = new StubRedisRepository(
                new RedisSearchResult(new[] { new RedisKey("x") }, cursor: 0) { NodeCursors = nodemap });

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "*",
                Cursor = "0",
                PageSize = 10
            };

            var result = await RedisKeysSearchQueryHandler.Handle(query, stub);

            result.NodeCursors.ShouldNotBeNull();
            result.NodeCursors["1:6379"].ShouldBe(5);
        }

        [Fact]
        public async Task Handle_RepositoryThrowsKeyNotFoundException_Propagates()
        {
            var stub = new ThrowingRedisRepository();
            var query = new RedisKeysSearchQuery { GroupId = Guid.NewGuid() };

            await Should.ThrowAsync<KeyNotFoundException>(
                () => RedisKeysSearchQueryHandler.Handle(query, stub));
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var stub = new StubRedisRepository(
                new RedisSearchResult(Array.Empty<RedisKey>(), 0));

            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisKeysSearchQueryHandler.Handle(null!, stub));
        }

        private sealed class StubRedisRepository : IRedisRepository
        {
            private readonly RedisSearchResult _result;
            public StubRedisRepository(RedisSearchResult result) => _result = result;

            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string cursor, int pageSize) =>
                Task.FromResult(_result);

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                Task.FromResult<IReadOnlyCollection<RedisServerNode>>(Array.Empty<RedisServerNode>());

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                Task.FromResult(new RedisInfo(new Dictionary<string, IReadOnlyDictionary<string, string>>()));
        }

        private sealed class ThrowingRedisRepository : IRedisRepository
        {
            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string cursor, int pageSize) =>
                throw new KeyNotFoundException("group not found");

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                throw new NotImplementedException();

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                throw new NotImplementedException();
        }
    }
}
