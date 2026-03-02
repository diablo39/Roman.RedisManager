using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class GetKeyValueQueryHandlerTests
    {
        [Fact]
        public async Task Handle_StringKey_ReturnsStringValue()
        {
            var keyValue = new RedisKeyValue(RedisDataType.String, stringValue: "hello");
            var stub = new StubKeyRepository(keyValue);
            var query = new GetKeyValueQuery { GroupId = Guid.NewGuid(), Key = "test:string" };

            var result = await GetKeyValueQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Type.ShouldBe("String");
            result.StringValue.ShouldBe("hello");
            result.ListValues.ShouldBeNull();
            result.SetMembers.ShouldBeNull();
            result.HashFields.ShouldBeNull();
            result.SortedSetEntries.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_ListKey_ReturnsListValues()
        {
            var keyValue = new RedisKeyValue(RedisDataType.List, listValues: new[] { "a", "b", "c" });
            var stub = new StubKeyRepository(keyValue);
            var query = new GetKeyValueQuery { GroupId = Guid.NewGuid(), Key = "test:list" };

            var result = await GetKeyValueQueryHandler.Handle(query, stub);

            result.Type.ShouldBe("List");
            result.ListValues.ShouldNotBeNull();
            result.ListValues!.Count.ShouldBe(3);
            result.StringValue.ShouldBeNull();
            result.SetMembers.ShouldBeNull();
            result.HashFields.ShouldBeNull();
            result.SortedSetEntries.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_SortedSetKey_ReturnsSortedSetEntries()
        {
            var entries = new[] { new RedisSortedSetEntry("alice", 1.0), new RedisSortedSetEntry("bob", 2.5) };
            var keyValue = new RedisKeyValue(RedisDataType.SortedSet, sortedSetEntries: entries);
            var stub = new StubKeyRepository(keyValue);
            var query = new GetKeyValueQuery { GroupId = Guid.NewGuid(), Key = "test:zset" };

            var result = await GetKeyValueQueryHandler.Handle(query, stub);

            result.Type.ShouldBe("SortedSet");
            result.SortedSetEntries.ShouldNotBeNull();
            result.SortedSetEntries!.Count.ShouldBe(2);
            result.SortedSetEntries.ShouldContain(e => e.Member == "alice" && e.Score == 1.0);
            result.SortedSetEntries.ShouldContain(e => e.Member == "bob" && e.Score == 2.5);
            result.StringValue.ShouldBeNull();
            result.ListValues.ShouldBeNull();
            result.SetMembers.ShouldBeNull();
            result.HashFields.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_NonExistentKey_ReturnsNoneType()
        {
            var keyValue = new RedisKeyValue(RedisDataType.None);
            var stub = new StubKeyRepository(keyValue);
            var query = new GetKeyValueQuery { GroupId = Guid.NewGuid(), Key = "missing:key" };

            var result = await GetKeyValueQueryHandler.Handle(query, stub);

            result.Type.ShouldBe("None");
            result.StringValue.ShouldBeNull();
            result.ListValues.ShouldBeNull();
            result.SetMembers.ShouldBeNull();
            result.HashFields.ShouldBeNull();
            result.SortedSetEntries.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_SetKey_ReturnsSetMembers()
        {
            var keyValue = new RedisKeyValue(RedisDataType.Set, setMembers: new[] { "a", "b", "c" });
            var stub = new StubKeyRepository(keyValue);
            var query = new GetKeyValueQuery { GroupId = Guid.NewGuid(), Key = "test:set" };

            var result = await GetKeyValueQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Type.ShouldBe("Set");
            result.SetMembers.ShouldNotBeNull();
            result.SetMembers!.Count.ShouldBe(3);
            result.SetMembers.ShouldContain("a");
            result.SetMembers.ShouldContain("b");
            result.SetMembers.ShouldContain("c");
            result.StringValue.ShouldBeNull();
            result.ListValues.ShouldBeNull();
            result.HashFields.ShouldBeNull();
            result.SortedSetEntries.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_HashKey_ReturnsHashFields()
        {
            var hashFields = new Dictionary<string, string> { ["name"] = "Alice", ["age"] = "30" };
            var keyValue = new RedisKeyValue(RedisDataType.Hash, hashFields: hashFields);
            var stub = new StubKeyRepository(keyValue);
            var query = new GetKeyValueQuery { GroupId = Guid.NewGuid(), Key = "test:hash" };

            var result = await GetKeyValueQueryHandler.Handle(query, stub);

            result.ShouldNotBeNull();
            result.Type.ShouldBe("Hash");
            result.HashFields.ShouldNotBeNull();
            result.HashFields!.Count.ShouldBe(2);
            result.HashFields["name"].ShouldBe("Alice");
            result.HashFields["age"].ShouldBe("30");
            result.StringValue.ShouldBeNull();
            result.ListValues.ShouldBeNull();
            result.SetMembers.ShouldBeNull();
            result.SortedSetEntries.ShouldBeNull();
        }

        [Fact]
        public async Task Handle_NullQuery_ThrowsArgumentNullException()
        {
            var stub = new StubKeyRepository(new RedisKeyValue(RedisDataType.None));

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetKeyValueQueryHandler.Handle(null!, stub));
        }

        [Fact]
        public async Task Handle_NullRepository_ThrowsArgumentNullException()
        {
            var query = new GetKeyValueQuery { GroupId = Guid.NewGuid(), Key = "k" };

            await Should.ThrowAsync<ArgumentNullException>(
                () => GetKeyValueQueryHandler.Handle(query, null!));
        }

        private sealed class StubKeyRepository : IRedisKeyRepository
        {
            private readonly RedisKeyValue _keyValue;

            public StubKeyRepository(RedisKeyValue keyValue) => _keyValue = keyValue;

            public Task<bool> DeleteKeyAsync(Guid groupId, string key) =>
                Task.FromResult(false);

            public Task<RedisKeyMetadata> GetKeyMetadataAsync(Guid groupId, string key) =>
                Task.FromResult(new RedisKeyMetadata(RedisDataType.None, null));

            public Task<RedisKeyValue> GetKeyValueAsync(Guid groupId, string key) =>
                Task.FromResult(_keyValue);
        }
    }
}
