using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    [Collection("Redis")]
    public class RedisRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task SearchForKeysAsync_ShouldReturnNonNullAndNonEmptyKeys()
        {
            var redisContainer = _fixture.Container;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            // seed a key to ensure search returns something
            await connectionMultiplexer.GetDatabase().StringSetAsync("seed", "value");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Name = "placeholder",
                        ConnectionString = redisContainer.GetConnectionString(),
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

        [Fact]
        public async Task GetInfoAsync_ShouldReturnStructuredInfo()
        {
            var redisContainer = _fixture.Container;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        Name = "placeholder",
                        ConnectionString = redisContainer.GetConnectionString(),
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(connectionMultiplexer);

            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            // Act
            // use the actual host/port published by the container
            var endpointParts = redisContainer.GetConnectionString().Split(':');
            var host = endpointParts[0];
            var port = int.Parse(endpointParts[1]);
            RedisInfo info = await redisRepository.GetInfoAsync(groupId, host, port);

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
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new SimpleConnectionManager(ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true"));
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
