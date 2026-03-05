using System.Net;
using System.Threading.Tasks;
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

        [Fact]
        public async Task UnauthorizedEndpoint_Returns401WithProblemDetails()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/test/unauth");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldBeValidProblemDetails();
        }

        [Fact]
        public async Task ForbiddenEndpoint_Returns403WithProblemDetails()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/test/forbidden");

            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldBeValidProblemDetails();
        }

        [Fact]
        public async Task ServerErrorEndpoint_Returns500WithProblemDetails()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/test/server-error");

            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldBeValidProblemDetails();
        }
    }
}
