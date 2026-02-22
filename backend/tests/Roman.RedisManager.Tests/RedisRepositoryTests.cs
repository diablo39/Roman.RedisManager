using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests
{
    public class RedisRepositoryTests
    {
        [Fact(Skip = "Requires local Redis instance")]
        public async Task SearchForKeysAsync_ShouldReturnNonNullAndNonEmptyKeys()
        {
            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Name = "placeholder",
                        ConnectionString = "localhost:6379",
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(connectionMultiplexer);

            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            // Act
            RedisSearchResult result = await redisRepository.SearchForKeysAsync(groupId, predicate: string.Empty);

            // Assert
            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
        }

        [Fact(Skip = "Requires local Redis instance")]
        public async Task GetInfoAsync_ShouldReturnStructuredInfo()
        {
            // Arrange
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        Name = "placeholder",
                        ConnectionString = "localhost:6379",
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(connectionMultiplexer);

            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            // Act
            RedisInfo info = await redisRepository.GetInfoAsync(groupId, "localhost", 6379);

            // Assert
            info.ShouldNotBeNull();
            info.Sections.ShouldNotBeEmpty();
            info.Sections.ContainsKey("Server").ShouldBeTrue();
        }

        [Fact]
        public async Task GetInfoAsync_InvalidArguments_ThrowArgumentException()
        {
            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        Name = "placeholder",
                        ConnectionString = "localhost:6379",
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(ConnectionMultiplexer.Connect("localhost:6379"));
            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            await Should.ThrowAsync<ArgumentException>(
                () => redisRepository.GetInfoAsync(Guid.Empty, "localhost", 6379));

            await Should.ThrowAsync<ArgumentException>(
                () => redisRepository.GetInfoAsync(groupId, "", 6379));

            await Should.ThrowAsync<ArgumentException>(
                () => redisRepository.GetInfoAsync(groupId, "localhost", 0));
        }

        private sealed class SimpleConnectionManager : IRedisConnectionManager
        {
            private readonly IConnectionMultiplexer _multiplexer;

            public SimpleConnectionManager(IConnectionMultiplexer multiplexer) => _multiplexer = multiplexer;

            public Task<IConnectionMultiplexer> GetConnectionAsync(Guid groupId) => Task.FromResult(_multiplexer);

            public ValueTask DisposeAsync()
            {
                _multiplexer.Dispose();
                return ValueTask.CompletedTask;
            }
        }

    }
}
