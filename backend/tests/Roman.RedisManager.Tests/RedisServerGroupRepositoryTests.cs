using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Configuration;
using Roman.RedisManager.Infrastructure.Repositories;
using System.Linq;

namespace Roman.RedisManager.Tests
{
    public class RedisServerGroupRepositoryTests
    {
        [Fact]
        public void ListRedisServerGroups_WhenCalled_ReturnsNonNullAndNonEmptyCollection()
        {
            // Arrange
            IRedisServerGroupRepository repository = new RedisServerGroupRepository(Options.Create(GetConfiguration()));

            // Act
            var result = repository.ListRedisServerGroups();

            // Assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public void ListRedisServerGroups_ReturnedServerGroups_HaveNameAndEndpoints()
        {
            // Arrange
            IRedisServerGroupRepository repository = new RedisServerGroupRepository(Options.Create(GetConfiguration()));

            // Act
            var serverGroups = repository.ListRedisServerGroups().ToList();

            // Assert
            serverGroups.ShouldAllBe(serverGroup => !string.IsNullOrWhiteSpace(serverGroup.Name));
            serverGroups.ShouldAllBe(serverGroup => !string.IsNullOrWhiteSpace(serverGroup.ConnectionString));

            // Additional check for connection string format
            serverGroups.Select(s => s.ConnectionString).ShouldContain(e => e.Contains(':'));
        }

        private RedisConfiguration GetConfiguration()
        {
            return new RedisConfiguration
            {
                ServerGroups = new[]
                {
                    new RedisServerGroupConfiguration
                    {
                        Name = "Test server 1",
                        ConnectionString = "localhost:6379",
                        GroupType = GroupType.Standalone
                    },
                    new RedisServerGroupConfiguration
                    {
                        Name = "Test server 2",
                        ConnectionString = "localhost:16379",
                        GroupType = GroupType.Cluster
                    }
                }
            };
        }
    }
}
