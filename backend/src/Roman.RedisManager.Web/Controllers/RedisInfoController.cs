using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Web.Wolverine;
using System;
using System.Threading.Tasks;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    [ApiController]
    [Route("api/commands/redis-info")]
    public class RedisInfoController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        [HttpGet]
        public async Task<RedisInfoQueryResult> GetInfo([FromQuery] Guid groupId, [FromQuery] string host, [FromQuery] int port)
        {
            return await _bus.InvokeAsync<RedisInfoQueryResult>(
                new RedisInfoQuery { GroupId = groupId, Host = host, Port = port });
        }
    }
}
