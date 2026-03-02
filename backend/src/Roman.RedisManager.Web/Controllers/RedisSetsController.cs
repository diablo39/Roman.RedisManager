using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Infrastructure.Exceptions;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    public record AddToSetRequest(
        Guid GroupId,
        string Key,
        IReadOnlyCollection<string> Members,
        TimeSpan? Ttl = null);

    public record RemoveFromSetRequest(
        Guid GroupId,
        string Key,
        IReadOnlyCollection<string> Members);

    [Route("api/redis-sets")]
    [ApiController]
    public class RedisSetsController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddToSetCommandResult>> AddToSet([FromBody] AddToSetRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<AddToSetCommandResult>(
                    new AddToSetCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Members = request.Members,
                        Ttl = request.Ttl
                    });

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (RedisTypeMismatchException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (RedisConnectionFailureException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetSetMembersQueryResult>> GetSetMembers(
            [FromQuery] Guid groupId,
            [FromQuery] string key,
            [FromQuery] long cursor = 0,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var result = await _bus.InvokeAsync<GetSetMembersQueryResult>(
                    new GetSetMembersQuery
                    {
                        GroupId = groupId,
                        Key = key,
                        Cursor = cursor,
                        PageSize = pageSize
                    });

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (RedisTypeMismatchException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (RedisConnectionFailureException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RemoveFromSetCommandResult>> RemoveFromSet([FromBody] RemoveFromSetRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<RemoveFromSetCommandResult>(
                    new RemoveFromSetCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Members = request.Members
                    });

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (RedisTypeMismatchException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (RedisConnectionFailureException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
