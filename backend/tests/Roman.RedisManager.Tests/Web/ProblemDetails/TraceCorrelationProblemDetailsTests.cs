using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Net.Http.Json;
using Roman.RedisManager.Web;
using Xunit;

namespace Roman.RedisManager.Tests.Web.ProblemDetails
{
    public class TraceCorrelationProblemDetailsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public TraceCorrelationProblemDetailsTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task ProblemDetailsContainsTraceIdAndRequestPath()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/test/unauth");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.Extensions.ShouldContainKey("traceId");
            problem.Extensions.ShouldContainKey("requestPath");
            GetExtensionString(problem, "requestPath").ShouldBe("/api/test/unauth");
        }

        [Fact]
        public async Task ProblemDetailsIncludesTraceparentIfProvided()
        {
            var client = _factory.CreateClient();
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/api/test/unauth");
            req.Headers.Add("traceparent", "00-11111111111111111111111111111111-2222222222222222-01");
            var response = await client.SendAsync(req);

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.Extensions.ShouldContainKey("traceparent");
            GetExtensionString(problem, "traceparent").ShouldBe("00-11111111111111111111111111111111-2222222222222222-01");
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
