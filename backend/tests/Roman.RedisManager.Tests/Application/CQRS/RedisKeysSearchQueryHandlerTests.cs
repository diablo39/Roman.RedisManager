using Microsoft.Extensions.Options;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Configuration;
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

            // Arrange
            var stub = new StubRedisRepository(
                new RedisSearchResult(
                    new[]
                    {
                        new RedisKey("key:1", RedisDataType.String, TimeSpan.FromMinutes(5), true),
                        new RedisKey("key:2", RedisDataType.Hash, null, false)
                    },
                    hasMoreResults: true)
                {
                    ContinuationToken = "token-42"
                });

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "key:*",
                ContinuationToken = null,
                PageSize = 10
            };

            // Act
            var result = await RedisKeysSearchQueryHandler.Handle(query, stub, CreateSearchLimitsOptions(100));

            // Assert
            result.ShouldNotBeNull();
            result.Keys.Count.ShouldBe(2);
            result.Keys.ShouldContain(k => k.Key == "key:1");
            result.Keys.ShouldContain(k => k.Key == "key:2");
            result.Keys.ShouldContain(k => k.Key == "key:1" && k.Type == "String" && k.TtlMilliseconds == 300000 && k.HasExpiration);
            result.Keys.ShouldContain(k => k.Key == "key:2" && k.Type == "Hash" && k.TtlMilliseconds == null && !k.HasExpiration);
            result.HasMoreResults.ShouldBeTrue();
            result.ContinuationToken.ShouldBe("token-42");
        }

        [Fact]
        public async Task Handle_WithZeroCursorReturned_HasMoreResultsIsFalse()
        {

            // Arrange
            var stub = new StubRedisRepository(
                new RedisSearchResult(new[] { new RedisKey("only:key") }, cursor: 0));

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "*",
                ContinuationToken = null,
                PageSize = 100
            };

            // Act
            var result = await RedisKeysSearchQueryHandler.Handle(query, stub, CreateSearchLimitsOptions(100));

            // Assert
            result.HasMoreResults.ShouldBeFalse();
            result.Keys.Count.ShouldBe(1);
            result.ContinuationToken.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_WithMoreResults_MapsContinuationToken()
        {

            // Arrange
            var stub = new StubRedisRepository(
                new RedisSearchResult(new[] { new RedisKey("x") }, hasMoreResults: true)
                {
                    ContinuationToken = "next-page-token"
                });

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "*",
                ContinuationToken = null,
                PageSize = 10
            };

            // Act
            var result = await RedisKeysSearchQueryHandler.Handle(query, stub, CreateSearchLimitsOptions(100));

            // Assert
            result.HasMoreResults.ShouldBeTrue();
            result.ContinuationToken.ShouldBe("next-page-token");
        }

        [Fact]
        public async Task Handle_WhenRepositoryMarksCompletion_ContinuationTokenIsNull()
        {

            // Arrange
            var stub = new StubRedisRepository(
                new RedisSearchResult(new[] { new RedisKey("final:key") }, hasMoreResults: false)
                {
                    ContinuationToken = "should-not-be-returned"
                });

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "*",
                PageSize = 10
            };

            // Act
            var result = await RedisKeysSearchQueryHandler.Handle(query, stub, CreateSearchLimitsOptions(100));

            // Assert
            result.HasMoreResults.ShouldBeFalse();
            result.ContinuationToken.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_WithEmptyResultsAndCompletion_ContinuationTokenIsNull()
        {

            // Arrange
            var stub = new StubRedisRepository(
                new RedisSearchResult(Array.Empty<RedisKey>(), hasMoreResults: false));

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "missing:*",
                PageSize = 10
            };

            // Act
            var result = await RedisKeysSearchQueryHandler.Handle(query, stub, CreateSearchLimitsOptions(100));

            // Assert
            result.Keys.ShouldBeEmpty();
            result.HasMoreResults.ShouldBeFalse();
            result.ContinuationToken.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_RepositoryThrowsKeyNotFoundException_Propagates()
        {

            // Arrange
            var stub = new ThrowingRedisRepository();
            var query = new RedisKeysSearchQuery { GroupId = Guid.NewGuid() };

            // Act

            // Assert
            await Should.ThrowAsync<KeyNotFoundException>(
                () => RedisKeysSearchQueryHandler.Handle(query, stub, CreateSearchLimitsOptions(100)));
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubRedisRepository(
                new RedisSearchResult(Array.Empty<RedisKey>(), 0));

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisKeysSearchQueryHandler.Handle(null!, stub, CreateSearchLimitsOptions(100)));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {

            // Arrange
            var query = new RedisKeysSearchQuery { GroupId = Guid.NewGuid() };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisKeysSearchQueryHandler.Handle(query, null!, CreateSearchLimitsOptions(100)));
        }

        [Fact]
        public async Task Handle_WithOversizedPageSize_CapsRequestToConfiguredLimit()
        {

            // Arrange
            var trackingRepository = new TrackingRedisRepository(
                new RedisSearchResult(new[] { new RedisKey("cap:key") }, hasMoreResults: false));
            var expectedGroupId = Guid.NewGuid();

            var query = new RedisKeysSearchQuery
            {
                GroupId = expectedGroupId,
                Pattern = "cap:*",
                ContinuationToken = "continuation-01",
                PageSize = 5000
            };

            // Act
            await RedisKeysSearchQueryHandler.Handle(query, trackingRepository, CreateSearchLimitsOptions(50));

            // Assert
            trackingRepository.LastRequestedPageSize.ShouldBe(50);
            trackingRepository.LastRequestedGroupId.ShouldBe(expectedGroupId);
            trackingRepository.LastRequestedPattern.ShouldBe("cap:*");
            trackingRepository.LastRequestedContinuationToken.ShouldBe("continuation-01");
        }

        [Fact]
        public async Task Handle_WithNonPositivePageSize_UsesConfiguredLimit()
        {

            // Arrange
            var trackingRepository = new TrackingRedisRepository(
                new RedisSearchResult(Array.Empty<RedisKey>(), hasMoreResults: false));

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "*",
                PageSize = 0
            };

            // Act
            await RedisKeysSearchQueryHandler.Handle(query, trackingRepository, CreateSearchLimitsOptions(25));

            // Assert
            trackingRepository.LastRequestedPageSize.ShouldBe(25);
        }

        [Fact]
        public async Task Handle_WithDefaultPageSize_UsesQueryDefaultWhenUnderConfiguredLimit()
        {

            // Arrange
            var trackingRepository = new TrackingRedisRepository(
                new RedisSearchResult(Array.Empty<RedisKey>(), hasMoreResults: false));

            var query = new RedisKeysSearchQuery
            {
                GroupId = Guid.NewGuid(),
                Pattern = "default:*"
            };

            // Act
            await RedisKeysSearchQueryHandler.Handle(query, trackingRepository, CreateSearchLimitsOptions(200));

            // Assert
            trackingRepository.LastRequestedPageSize.ShouldBe(100);
        }

        [Fact]
        public async Task Handle_NullLimitsOptions_ThrowsArgumentNullException()
        {

            // Arrange
            var stub = new StubRedisRepository(new RedisSearchResult(Array.Empty<RedisKey>(), hasMoreResults: false));
            var query = new RedisKeysSearchQuery { GroupId = Guid.NewGuid() };

            // Act

            // Assert
            await Should.ThrowAsync<ArgumentNullException>(
                () => RedisKeysSearchQueryHandler.Handle(query, stub, null!));
        }

        private static IOptions<RedisSearchLimitsConfiguration> CreateSearchLimitsOptions(int maxPageSize) =>
            Options.Create(new RedisSearchLimitsConfiguration { MaxPageSize = maxPageSize });

        private sealed class StubRedisRepository : IRedisRepository
        {
            private readonly RedisSearchResult _result;
            public StubRedisRepository(RedisSearchResult result) => _result = result;

            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string? continuationToken, int pageSize) =>
                Task.FromResult(_result);

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                Task.FromResult<IReadOnlyCollection<RedisServerNode>>(Array.Empty<RedisServerNode>());

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                Task.FromResult(new RedisInfo(new Dictionary<string, IReadOnlyDictionary<string, string>>()));
        }

        private sealed class TrackingRedisRepository : IRedisRepository
        {
            private readonly RedisSearchResult _result;

            public TrackingRedisRepository(RedisSearchResult result) => _result = result;

            public int LastRequestedPageSize { get; private set; }

            public Guid LastRequestedGroupId { get; private set; }

            public string? LastRequestedPattern { get; private set; }

            public string? LastRequestedContinuationToken { get; private set; }

            public Task<RedisSearchResult> SearchForKeysAsync(Guid groupId, string pattern, string? continuationToken, int pageSize)
            {
                LastRequestedGroupId = groupId;
                LastRequestedPattern = pattern;
                LastRequestedContinuationToken = continuationToken;
                LastRequestedPageSize = pageSize;
                return Task.FromResult(_result);
            }

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                throw new NotImplementedException();

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                throw new NotImplementedException();
        }

        private sealed class ThrowingRedisRepository : IRedisRepository
        {
            public Task<RedisSearchResult> SearchForKeysAsync(
                Guid groupId, string pattern, string? continuationToken, int pageSize) =>
                throw new KeyNotFoundException("group not found");

            public Task<IReadOnlyCollection<RedisServerNode>> GetServerNodesAsync(Guid groupId) =>
                throw new NotImplementedException();

            public Task<RedisInfo> GetInfoAsync(Guid groupId, string host, int port) =>
                throw new NotImplementedException();
        }
    }
}
