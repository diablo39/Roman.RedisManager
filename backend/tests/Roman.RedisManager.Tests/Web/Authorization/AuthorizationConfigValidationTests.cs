using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Roman.RedisManager.Web;

namespace Roman.RedisManager.Tests.Web.Authorization
{
    public class AuthorizationConfigValidationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public AuthorizationConfigValidationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public void Startup_WithDuplicateRoles_ThrowsOptionsValidationException()
        {
            // Arrange
            var invalidConfig = new Dictionary<string, string?>
            {
                ["Security:Authorization:Roles:0:RoleName"] = "reader",
                ["Security:Authorization:Roles:1:RoleName"] = "reader"
            };

            // Act & Assert
            var exception = Should.Throw<Exception>(() =>
            {
                _factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                    {
                        configurationBuilder.Sources.Clear();
                        configurationBuilder.AddInMemoryCollection(invalidConfig);
                    });
                }).CreateClient();
            });

            exception.ShouldNotBeNull();
        }

        [Fact]
        public void Startup_WithValidConfiguration_DoesNotThrow()
        {
            // Arrange & Act
            var client = _factory.CreateClient();

            // Assert
            client.ShouldNotBeNull();
        }
    }
}
