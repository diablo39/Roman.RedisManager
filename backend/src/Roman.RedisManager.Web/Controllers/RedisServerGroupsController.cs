using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Web.Wolverine;
using System.Collections.Generic;
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

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RedisServerGroupDetailQueryResult>> GetServerGroupDetail(string id)
        {
            try
            {
                var result = await _bus.InvokeAsync<RedisServerGroupDetailQueryResult>(
                    new RedisServerGroupDetailQuery { Id = id });

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (RedisConnectionFailureException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
