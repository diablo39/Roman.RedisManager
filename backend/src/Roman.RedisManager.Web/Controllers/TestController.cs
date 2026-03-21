using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Roman.RedisManager.Web.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/test")]
    [ApiExplorerSettings(IgnoreApi = true)] // hide from Swagger/OpenAPI
    public class TestController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public TestController(IWebHostEnvironment env) => _env = env;

        [HttpGet("unauth")]
        public IActionResult GetUnauthenticated()
        {
            if (!_env.IsDevelopment())
                return NotFound();
            return Unauthorized();
        }

        [HttpGet("forbidden")]
        public IActionResult GetForbidden()
        {
            if (!_env.IsDevelopment())
                return NotFound();
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        [HttpGet("server-error")]
        public IActionResult GetServerError()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            // Throwing exception triggers the global exception handler, resulting in problem details
            throw new System.Exception("simulated test failure");
        }
    }
}
