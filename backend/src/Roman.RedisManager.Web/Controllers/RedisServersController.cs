using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Web.Wolverine;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisServersController : ControllerBase
    {
        // GET: api/RedisServers
        private readonly IMessageBus _bus;

        public RedisServersController(IMessageBus bus) => _bus = bus;

        [HttpGet]
        public async Task<RedisServersQueryResult> GetServers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await _bus.InvokeAsync<RedisServersQueryResult>(
                new RedisServersQuery { PageNumber = pageNumber, PageSize = pageSize });
        }
    }
}
