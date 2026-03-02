using Microsoft.Extensions.Options;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.Server;

namespace Roman.RedisManager.Tests.Application.CQRS
{
    public class RedisServerGroupsQueryHandlerTests
    {
        [Fact]
        public void Handle_WithMultipleGroups_FirstPage_ReturnsCorrectSlice()
        {
            var options = CreateOptions(CreateGroups(5));
            var query = new RedisServerGroupsQuery { PageNumber = 1, PageSize = 2 };

            var result = RedisServerGroupsQueryHandler.Handle(query, options);

            result.ServerGroups.Count.ShouldBe(2);
            result.ServerGroups[0].Name.ShouldBe("Group-0");
            result.ServerGroups[1].Name.ShouldBe("Group-1");
            result.TotalCount.ShouldBe(5);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(2);
        }

        [Fact]
        public void Handle_WithMultipleGroups_SecondPage_ReturnsCorrectSlice()
        {
            var options = CreateOptions(CreateGroups(5));
            var query = new RedisServerGroupsQuery { PageNumber = 2, PageSize = 2 };

            var result = RedisServerGroupsQueryHandler.Handle(query, options);

            result.ServerGroups.Count.ShouldBe(2);
            result.ServerGroups[0].Name.ShouldBe("Group-2");
            result.ServerGroups[1].Name.ShouldBe("Group-3");
            result.TotalCount.ShouldBe(5);
            result.PageNumber.ShouldBe(2);
            result.PageSize.ShouldBe(2);
        }

        [Fact]
        public void Handle_PageBeyondEnd_ReturnsEmptyPage()
        {
            var options = CreateOptions(CreateGroups(3));
            var query = new RedisServerGroupsQuery { PageNumber = 99, PageSize = 10 };

            var result = RedisServerGroupsQueryHandler.Handle(query, options);

            result.ServerGroups.ShouldBeEmpty();
            result.TotalCount.ShouldBe(3);
            result.PageNumber.ShouldBe(99);
            result.PageSize.ShouldBe(10);
        }

        [Fact]
        public void Handle_TotalCount_AlwaysReflectsFullListLength()
        {
            var options = CreateOptions(CreateGroups(7));
            var query = new RedisServerGroupsQuery { PageNumber = 1, PageSize = 3 };

            var result = RedisServerGroupsQueryHandler.Handle(query, options);

            result.TotalCount.ShouldBe(7);
            result.ServerGroups.Count.ShouldBe(3);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(3);
        }

        [Fact]
        public void Handle_WithEmptyServerGroups_ReturnsEmptyResult()
        {
            var options = CreateOptions(Array.Empty<RedisServerGroupConfiguration>());
            var query = new RedisServerGroupsQuery { PageNumber = 1, PageSize = 10 };

            var result = RedisServerGroupsQueryHandler.Handle(query, options);

            result.ServerGroups.ShouldBeEmpty();
            result.TotalCount.ShouldBe(0);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(10);
        }

        [Fact]
        public void Handle_Mapping_GroupTypeAndIdForwardedCorrectly()
        {
            var id = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            var options = Options.Create(new RedisConfiguration
            {
                ServerGroups = new[]
                {
                    new RedisServerGroupConfiguration
                    {
                        Id = id,
                        Name = "mapped-group",
                        ConnectionString = "localhost:6379",
                        GroupType = GroupType.Cluster
                    }
                }
            });
            var query = new RedisServerGroupsQuery { PageNumber = 1, PageSize = 10 };

            var result = RedisServerGroupsQueryHandler.Handle(query, options);

            result.ServerGroups.Count.ShouldBe(1);
            result.ServerGroups[0].Id.ShouldBe(id);
            result.ServerGroups[0].Name.ShouldBe("mapped-group");
            result.ServerGroups[0].GroupType.ShouldBe(GroupType.Cluster);
            result.TotalCount.ShouldBe(1);
            result.PageNumber.ShouldBe(1);
            result.PageSize.ShouldBe(10);
        }

        private static IOptions<RedisConfiguration> CreateOptions(
            IEnumerable<RedisServerGroupConfiguration> groups) =>
            Options.Create(new RedisConfiguration { ServerGroups = groups });

        private static RedisServerGroupConfiguration[] CreateGroups(int count) =>
            Enumerable.Range(0, count)
                .Select(i => new RedisServerGroupConfiguration
                {
                    Id = Guid.NewGuid(),
                    Name = $"Group-{i}",
                    ConnectionString = "localhost:6379",
                    GroupType = GroupType.Standalone
                })
                .ToArray();
    }
}
