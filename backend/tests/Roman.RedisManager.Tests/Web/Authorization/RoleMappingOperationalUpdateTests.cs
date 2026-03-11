using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Roman.RedisManager.Tests.Web.Helpers;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class RoleMappingOperationalUpdateTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RoleMappingOperationalUpdateTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task ReadEndpoint_WhenRoleClaimMappingUpdatedInConfiguration_AllowsWithoutCodeChange()
        {
            // Arrange
            var tokenClaims = new Dictionary<string, object>
            {
                ["groups"] = "ops_readers"
            };

            var baselineClient = _factory.CreateClient();
            TestAuthTokenFactory.ApplyBearer(baselineClient, "https://login.microsoftonline.com/common/v2.0", "entra", tokenClaims);

            var overrideSettings = new Dictionary<string, string?>
            {
                ["Security:Authorization:RoleClaimMappings:0:AllowedValues:0"] = "ops_readers"
            };

            var updatedClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(overrideSettings);
                });
            }).CreateClient();

            TestAuthTokenFactory.ApplyBearer(updatedClient, "https://login.microsoftonline.com/common/v2.0", "entra", tokenClaims);

            // Act
            var baselineResponse = await baselineClient.GetAsync("/api/redis-server-groups");
            var updatedResponse = await updatedClient.GetAsync("/api/redis-server-groups");

            // Assert
            baselineResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            updatedResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
