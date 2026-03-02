using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;
using Roman.RedisManager.Domain.Entities.Server;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    [Collection("Redis")]
    public class RedisRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task SearchForKeysAsync_WithSeededKey_ReturnsNonEmptyResult()
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

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);

            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            // Act
            RedisSearchResult result = await redisRepository.SearchForKeysAsync(
                groupId, pattern: "*", cursor: "0", pageSize: 50);

            // Assert
            result.ShouldNotBeNull();
            result.Keys.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task GetServerNodesAsync_WithValidGroupId_ReturnsAtLeastOneNode()
        {
            var redisContainer = _fixture.Container;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                        Name = "placeholder",
                        ConnectionString = redisContainer.GetConnectionString(),
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);

            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            // Act
            var nodes = await redisRepository.GetServerNodesAsync(groupId);

            // Assert
            nodes.ShouldNotBeNull();
            nodes.ShouldNotBeEmpty();
            nodes.All(n => n.Role == "master" || n.Role == "slave").ShouldBeTrue();
        }

        [Fact]
        public async Task GetServerNodesAsync_InvalidGroupId_ThrowsKeyNotFoundException()
        {
            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                        Name = "placeholder",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true"));
            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);

            await Should.ThrowAsync<KeyNotFoundException>(() => redisRepository.GetServerNodesAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetServerNodesAsync_ClusterGroup_ReturnsNodesSameAsStandalone()
        {
            // cluster behaviour currently mirrors standalone; container does not actually run a cluster,
            // but the lookup logic should still return results when GroupType.Cluster is specified.
            var redisContainer = _fixture.Container;
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                        Name = "placeholder",
                        ConnectionString = redisContainer.GetConnectionString(),
                        GroupType = GroupType.Cluster
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            // Act
            var nodes = await redisRepository.GetServerNodesAsync(groupId);

            // Assert
            nodes.ShouldNotBeNull();
            nodes.ShouldNotBeEmpty();
            nodes.All(n => n.Role == "master" || n.Role == "slave").ShouldBeTrue();
        }

        [Fact]
        public async Task GetInfoAsync_WithValidGroupId_ReturnsStructuredSections()
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

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);

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

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true"));
            IRedisRepository redisRepository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            await Should.ThrowAsync<ArgumentException>(
                () => redisRepository.GetInfoAsync(Guid.Empty, "localhost", 6379));

            await Should.ThrowAsync<ArgumentException>(
                () => redisRepository.GetInfoAsync(groupId, "", 6379));

            await Should.ThrowAsync<ArgumentException>(
                () => redisRepository.GetInfoAsync(groupId, "localhost", 0));
        }

        [Fact]
        public async Task GetServerNodesAsync_WithStandaloneContainer_ReturnsNodeWithMasterRole()
        {
            // A single-node Redis container is never a replica, so IsReplica == false
            // and the repository must map that to role "master".
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
                        Name = "role-test",
                        ConnectionString = _fixture.Container.GetConnectionString(),
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var nodes = await repository.GetServerNodesAsync(groupId);

            nodes.ShouldNotBeNull();
            nodes.ShouldNotBeEmpty();
            nodes.ShouldContain(n => n.Role == "master");
        }

        [Fact]
        public async Task SearchForKeysAsync_WithSmallPageSize_ReturnsBoundedPage()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();

            for (int i = 0; i < 20; i++)
            {
                await db.StringSetAsync($"paging-test:{i}", "v");
            }

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        Name = "placeholder",
                        ConnectionString = _fixture.Container.GetConnectionString(),
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var result = await repository.SearchForKeysAsync(
                groupId, pattern: "paging-test:*", cursor: "0", pageSize: 5);

            result.ShouldNotBeNull();
            result.Keys.Count().ShouldBeLessThanOrEqualTo(5);
        }

        [Fact]
        public async Task SearchForKeysAsync_WithInitialCursor_CanIterateThroughKeys()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();

            for (int i = 0; i < 30; i++)
            {
                await db.StringSetAsync($"cursor-chain:{i}", "v");
            }

            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        Name = "placeholder",
                        ConnectionString = _fixture.Container.GetConnectionString(),
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisRepository repository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            var page1 = await repository.SearchForKeysAsync(
                groupId, pattern: "cursor-chain:*", cursor: "0", pageSize: 5);

            page1.ShouldNotBeNull();

            if (page1.HasMoreResults)
            {
                var page2 = await repository.SearchForKeysAsync(
                    groupId, pattern: "cursor-chain:*", cursor: page1.Cursor.ToString(), pageSize: 5);

                page2.ShouldNotBeNull();
            }
        }

        [Fact]
        public async Task SearchForKeysAsync_ClusterMode_DefaultsAcrossMasters()
        {
            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                        Name = "placeholder",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Cluster
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(
                ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true"));
            IRedisRepository repository = new RedisRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;

            // We expect the call to succeed and return a result; since our test container
            // isn't actually clustered we won't exercise multiple masters, but we can at
            // least verify that the API no longer throws and that NodeCursors is present.
            var result = await repository.SearchForKeysAsync(groupId, pattern: "*", cursor: "0", pageSize: 50);
            result.ShouldNotBeNull();
            result.NodeCursors.ShouldNotBeNull();
        }

        [Fact]
        public async Task SearchForKeysAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                        Name = "placeholder",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });

            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(
                ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true"));
            IRedisRepository repository = new RedisRepository(connectionManager, options);

            await Should.ThrowAsync<ArgumentException>(() =>
                repository.SearchForKeysAsync(
                    Guid.Empty, pattern: "*", cursor: "0", pageSize: 100));
        }
    
}
}
