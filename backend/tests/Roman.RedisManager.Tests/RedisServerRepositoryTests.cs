using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Repositories;
using Roman.RedisManager.Web.Configuration;
using System.Linq;

namespace Roman.RedisManager.Tests
{
    public class RedisServerRepositoryTests
    {
        [Fact]
        public void ListRedisServers_WhenCalled_ReturnsNonNullAndNonEmptyCollection()
        {
            // Arrange
            IRedisServerRepository repository = new RedisServerRepository(Options.Create(GetConfiguration()));

            // Act
            var result = repository.ListRedisServers();

            // Assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public void ListRedisServers_ReturnedServers_HaveNameAndEndpoints()
        {
            // Arrange
            IRedisServerRepository repository = new RedisServerRepository(Options.Create(GetConfiguration()));

            // Act
            var servers = repository.ListRedisServers().ToList();

            // Assert
            servers.ShouldAllBe(server => !string.IsNullOrWhiteSpace(server.Name));
            servers.ShouldAllBe(server => server.Endpoints != null && server.Endpoints.Any());

            // Additional check for endpoint format
            servers.SelectMany(s => s.Endpoints).ShouldContain(e => e.Contains(':'));
        }

        private RedisConfiguration GetConfiguration()
        {
            return new RedisConfiguration
            {
                Servers = [
                    new RedisServerConfiguration
                    {
                        Name = "Test server 1",
                        Endpoints = ["localhost:6379"]
                    },
                    new RedisServerConfiguration
                    {
                        Name = "Test server 2",
                        Endpoints = ["localhost:16379"]
                    }
                ]
            };
        }
    }
}
