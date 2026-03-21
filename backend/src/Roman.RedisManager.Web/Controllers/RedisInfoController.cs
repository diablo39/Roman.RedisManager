using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Web.Authorization;
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

        /// <summary>
        /// Gets Redis INFO command data for a specific Redis host in a server group.
        /// </summary>
        /// <param name="groupId">The Redis server group identifier.</param>
        /// <param name="host">The Redis host name or IP address.</param>
        /// <param name="port">The Redis TCP port.</param>
        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<RedisInfoQueryResult> GetInfo([FromQuery] Guid groupId, [FromQuery] string host, [FromQuery] int port)
        {
            return await _bus.InvokeAsync<RedisInfoQueryResult>(
                new RedisInfoQuery { GroupId = groupId, Host = host, Port = port });
        }
    }
}
