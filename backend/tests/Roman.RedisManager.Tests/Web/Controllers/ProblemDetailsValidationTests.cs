using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Roman.RedisManager.Web;
using Roman.RedisManager.Tests.Web.Helpers;

namespace Roman.RedisManager.Tests.Web.Controllers
{
    public class ProblemDetailsValidationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProblemDetailsValidationTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task InvalidInput_ReturnsValidationProblemDetails()
        {
            // Arrange
            var client = _factory.CreateClient();
            TestAuthTokenFactory.ApplyBearer(client, "redis-reader", "editor", "admin");

            // Act
            var response = await client.GetAsync("/api/redis-keys?groupId=not-a-guid");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var validation = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            validation.ShouldNotBeNull();
            validation.Errors.ShouldNotBeEmpty();
            validation.Extensions.ShouldContainKey("traceId");
        }

        [Fact]
        public async Task UnknownRoute_ReturnsProblemDetails404()
        {
            // Arrange
            var client = _factory.CreateClient();
            TestAuthTokenFactory.ApplyBearer(client, "admin");

            // Act
            var response = await client.GetAsync("/api/this-route-does-not-exist");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldNotBeNull();
            problem.ShouldBeValidProblemDetails();
        }
    }
}
