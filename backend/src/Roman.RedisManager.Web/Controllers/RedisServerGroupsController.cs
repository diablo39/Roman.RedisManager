using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Web.Wolverine;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    [Route("api/redis-server-groups")]
    [ApiController]
    public class RedisServerGroupsController : ControllerBase
    {
        // GET: api/redis-server-groups
        private readonly IMessageBus _bus;

        public RedisServerGroupsController(IMessageBus bus) => _bus = bus;

        [HttpGet]
        public async Task<RedisServerGroupsQueryResult> GetServerGroups([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await _bus.InvokeAsync<RedisServerGroupsQueryResult>(
                new RedisServerGroupsQuery { PageNumber = pageNumber, PageSize = pageSize });
        }
    }
}
