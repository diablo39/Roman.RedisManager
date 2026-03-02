using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Entities.Server;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Redis;
using Roman.RedisManager.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Repositories
{
    [Collection("Redis")]
    public class RedisKeyRepositoryTests
    {
        private readonly RedisContainerFixture _fixture;

        public RedisKeyRepositoryTests(RedisContainerFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task DeleteKeyAsync_ExistingKey_ReturnsTrue()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await connectionMultiplexer.GetDatabase().StringSetAsync("del:existing", "value");

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var deleted = await repo.DeleteKeyAsync(groupId, "del:existing");

            deleted.ShouldBeTrue();
        }

        [Fact]
        public async Task DeleteKeyAsync_NonExistentKey_ReturnsFalse()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var deleted = await repo.DeleteKeyAsync(groupId, "del:nonexistent:" + Guid.NewGuid());

            deleted.ShouldBeFalse();
        }

        [Fact]
        public async Task DeleteKeyAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisKeyRepository repo = new RedisKeyRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.DeleteKeyAsync(Guid.Empty, "some:key"));
        }

        [Fact]
        public async Task GetKeyMetadataAsync_StringKey_ReturnsStringType()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await connectionMultiplexer.GetDatabase().StringSetAsync("meta:string", "value");

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var metadata = await repo.GetKeyMetadataAsync(groupId, "meta:string");

            metadata.ShouldNotBeNull();
            metadata.Type.ShouldBe(RedisDataType.String);
            metadata.Ttl.ShouldBeNull();
        }

        [Fact]
        public async Task GetKeyMetadataAsync_WithTtl_ReturnsTtl()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await connectionMultiplexer.GetDatabase().StringSetAsync("meta:ttl", "value", TimeSpan.FromSeconds(30));

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var metadata = await repo.GetKeyMetadataAsync(groupId, "meta:ttl");

            metadata.Ttl.ShouldNotBeNull();
            metadata.Ttl!.Value.TotalMilliseconds.ShouldBeGreaterThan(0);
            metadata.Type.ShouldBe(RedisDataType.String);
        }

        [Fact]
        public async Task GetKeyMetadataAsync_WithoutTtl_ReturnsNullTtl()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await connectionMultiplexer.GetDatabase().StringSetAsync("meta:nottl", "value");

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var metadata = await repo.GetKeyMetadataAsync(groupId, "meta:nottl");

            metadata.Ttl.ShouldBeNull();
            metadata.Type.ShouldBe(RedisDataType.String);
        }

        [Fact]
        public async Task GetKeyValueAsync_StringKey_ReturnsStringValue()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await connectionMultiplexer.GetDatabase().StringSetAsync("val:string", "hello world");

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var keyValue = await repo.GetKeyValueAsync(groupId, "val:string");

            keyValue.ShouldNotBeNull();
            keyValue.Type.ShouldBe(RedisDataType.String);
            keyValue.StringValue.ShouldBe("hello world");
        }

        [Fact]
        public async Task GetKeyValueAsync_ListKey_ReturnsListValues()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            await db.KeyDeleteAsync("val:list");
            await db.ListRightPushAsync("val:list", new RedisValue[] { "a", "b", "c" });

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var keyValue = await repo.GetKeyValueAsync(groupId, "val:list");

            keyValue.Type.ShouldBe(RedisDataType.List);
            keyValue.ListValues.ShouldNotBeNull();
            keyValue.ListValues!.Count().ShouldBe(3);
            keyValue.ListValues.ShouldContain("a");
            keyValue.ListValues.ShouldContain("b");
            keyValue.ListValues.ShouldContain("c");
        }

        [Fact]
        public async Task GetKeyValueAsync_NonExistentKey_ReturnsNoneType()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var keyValue = await repo.GetKeyValueAsync(groupId, "val:nonexistent:" + Guid.NewGuid());

            keyValue.Type.ShouldBe(RedisDataType.None);
            keyValue.StringValue.ShouldBeNull();
            keyValue.ListValues.ShouldBeNull();
            keyValue.SetMembers.ShouldBeNull();
            keyValue.HashFields.ShouldBeNull();
            keyValue.SortedSetEntries.ShouldBeNull();
        }

        [Fact]
        public async Task GetKeyValueAsync_HashKey_ReturnsHashFields()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            var key = "val:hash:" + Guid.NewGuid();
            await db.KeyDeleteAsync(key);
            await db.HashSetAsync(key, new StackExchange.Redis.HashEntry[]
            {
                new("field1", "value1"),
                new("field2", "value2")
            });

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var keyValue = await repo.GetKeyValueAsync(groupId, key);

            keyValue.Type.ShouldBe(RedisDataType.Hash);
            keyValue.HashFields.ShouldNotBeNull();
            keyValue.HashFields!["field1"].ShouldBe("value1");
            keyValue.HashFields["field2"].ShouldBe("value2");
        }

        [Fact]
        public async Task GetKeyValueAsync_SetKey_ReturnsSetMembers()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            var key = "val:set:" + Guid.NewGuid();
            await db.KeyDeleteAsync(key);
            await db.SetAddAsync(key, new StackExchange.Redis.RedisValue[] { "member1", "member2" });

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var keyValue = await repo.GetKeyValueAsync(groupId, key);

            keyValue.Type.ShouldBe(RedisDataType.Set);
            keyValue.SetMembers.ShouldNotBeNull();
            keyValue.SetMembers!.ShouldContain("member1");
            keyValue.SetMembers.ShouldContain("member2");
        }

        [Fact]
        public async Task GetKeyValueAsync_SortedSetKey_ReturnsSortedSetEntries()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            var db = connectionMultiplexer.GetDatabase();
            var key = "val:zset:" + Guid.NewGuid();
            await db.KeyDeleteAsync(key);
            await db.SortedSetAddAsync(key, new StackExchange.Redis.SortedSetEntry[]
            {
                new("alpha", 1.0),
                new("beta", 2.0)
            });

            var (repo, groupId) = CreateRepository(connectionMultiplexer);

            var keyValue = await repo.GetKeyValueAsync(groupId, key);

            keyValue.Type.ShouldBe(RedisDataType.SortedSet);
            keyValue.SortedSetEntries.ShouldNotBeNull();
            keyValue.SortedSetEntries!.ShouldContain(e => e.Member == "alpha" && e.Score == 1.0);
            keyValue.SortedSetEntries.ShouldContain(e => e.Member == "beta" && e.Score == 2.0);
        }

        [Fact]
        public async Task GetKeyMetadataAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisKeyRepository repo = new RedisKeyRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.GetKeyMetadataAsync(Guid.Empty, "some:key"));
        }

        [Fact]
        public async Task GetKeyValueAsync_EmptyGroupId_ThrowsArgumentException()
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(_fixture.ConnectionString + ",allowAdmin=true");
            await using IRedisConnectionManager connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisKeyRepository repo = new RedisKeyRepository(connectionManager, CreateOptions());

            await Should.ThrowAsync<ArgumentException>(
                () => repo.GetKeyValueAsync(Guid.Empty, "some:key"));
        }

        private (IRedisKeyRepository repo, Guid groupId) CreateRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            var options = CreateOptions(connectionMultiplexer);
            var connectionManager = new TestRedisConnectionManager(connectionMultiplexer);
            IRedisKeyRepository repo = new RedisKeyRepository(connectionManager, options);
            var groupId = options.Value.ServerGroups.First().Id;
            return (repo, groupId);
        }

        private IOptions<RedisConfiguration> CreateOptions(IConnectionMultiplexer? _ = null)
        {
            return Options.Create(new RedisConfiguration
            {
                ServerGroups =
                [
                    new RedisServerGroupConfiguration
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        Name = "key-test",
                        ConnectionString = _fixture.ConnectionString,
                        GroupType = GroupType.Standalone
                    }
                ]
            });
        }
    
}
}
