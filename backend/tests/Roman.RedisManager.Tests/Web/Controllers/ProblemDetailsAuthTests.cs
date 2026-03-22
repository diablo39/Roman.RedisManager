using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net.Http.Json;
using Xunit;
using Roman.RedisManager.Web;
using Roman.RedisManager.Tests.Web.Helpers;

namespace Roman.RedisManager.Tests.Web.Controllers
{
    public class ProblemDetailsAuthTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProblemDetailsAuthTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Theory]
        [InlineData("/api/test/unauth", HttpStatusCode.Unauthorized)]
        [InlineData("/api/test/forbidden", HttpStatusCode.Forbidden)]
        [InlineData("/api/test/server-error", HttpStatusCode.InternalServerError)]
        public async Task TestEndpoint_Requested_ReturnsExpectedStatusWithProblemDetails(string endpoint, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(endpoint);

            // Assert
            response.StatusCode.ShouldBe(expectedStatusCode);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldNotBeNull();
            problem.ShouldBeValidProblemDetails();
            problem.Extensions.ShouldContainKey("requestPath");
            GetExtensionString(problem, "requestPath").ShouldBe(endpoint);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("00-11111111111111111111111111111111-2222222222222222-01")]
        public async Task UnauthorizedEndpoint_Requested_IncludesCorrelationData(string? traceparent)
        {
            // Arrange
            var client = _factory.CreateClient();
            HttpRequestMessage request = new(HttpMethod.Get, "/api/test/unauth");

            if (!string.IsNullOrWhiteSpace(traceparent))
            {
                request.Headers.Add("traceparent", traceparent);
            }

            // Act
            var response = await client.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldNotBeNull();
            problem.ShouldBeValidProblemDetails();
            problem.Extensions.ShouldContainKey("traceId");
            problem.Extensions.ShouldContainKey("requestPath");
            GetExtensionString(problem, "requestPath").ShouldBe("/api/test/unauth");

            if (!string.IsNullOrWhiteSpace(traceparent))
            {
                problem.Extensions.ShouldContainKey("traceparent");
                GetExtensionString(problem, "traceparent").ShouldBe(traceparent);
            }
        }

        private static string? GetExtensionString(Microsoft.AspNetCore.Mvc.ProblemDetails problem, string key)
        {
            var value = problem.Extensions[key];

            return value switch
            {
                JsonElement element when element.ValueKind == JsonValueKind.String => element.GetString(),
                _ => value?.ToString()
            };
        }
    }
}
