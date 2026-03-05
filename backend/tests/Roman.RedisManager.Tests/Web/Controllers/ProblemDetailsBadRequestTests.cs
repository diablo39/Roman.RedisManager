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
    public class ProblemDetailsBadRequestTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProblemDetailsBadRequestTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public async Task SearchKeys_InvalidGroupIdProducesProblemDetails400()
        {
            var client = _factory.CreateClient();

            // groupId is a Guid, provide invalid value to trigger model binding failure
            var response = await client.GetAsync("/api/redis-keys?groupId=not-a-guid");

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
            problem.ShouldBeValidProblemDetails();
        }
    }
}
