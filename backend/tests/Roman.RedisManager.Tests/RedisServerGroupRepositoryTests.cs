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
            serverGroups.ShouldAllBe(serverGroup => serverGroup.Endpoints != null && serverGroup.Endpoints.Any());

            // Additional check for endpoint format
            serverGroups.SelectMany(s => s.Endpoints).ShouldContain(e => e.Contains(':'));
        }

        private RedisConfiguration GetConfiguration()
        {
            return new RedisConfiguration
            {
                ServerGroups = [
                    new RedisServerGroupConfiguration
                    {
                        Name = "Test server 1",
                        Endpoints = ["localhost:6379"],
                        GroupType = GroupType.Standalone
                    },
                    new RedisServerGroupConfiguration
                    {
                        Name = "Test server 2",
                        Endpoints = ["localhost:16379"],
                        GroupType = GroupType.Cluster
                    }
                ]
            };
        }
    }
}
