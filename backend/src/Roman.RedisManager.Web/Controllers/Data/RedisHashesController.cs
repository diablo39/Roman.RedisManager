using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Infrastructure.Exceptions;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    public record SetHashFieldsRequest(
        Guid GroupId,
        string Key,
        IReadOnlyDictionary<string, string> Fields,
        TimeSpan? Ttl = null);

    public record RemoveHashFieldsRequest(
        Guid GroupId,
        string Key,
        IReadOnlyCollection<string> Fields);

    [Route("api/redis/data/hashes")]
    [ApiController]
    public class RedisHashesController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SetHashFieldsCommandResult>> SetHashFields([FromBody] SetHashFieldsRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<SetHashFieldsCommandResult>(
                    new SetHashFieldsCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Fields = request.Fields,
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
        public async Task<ActionResult<GetHashFieldsQueryResult>> GetHashFields(
            [FromQuery] Guid groupId,
            [FromQuery] string key,
            [FromQuery] long cursor = 0,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var result = await _bus.InvokeAsync<GetHashFieldsQueryResult>(
                    new GetHashFieldsQuery
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
        public async Task<ActionResult<RemoveHashFieldsCommandResult>> RemoveHashFields([FromBody] RemoveHashFieldsRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<RemoveHashFieldsCommandResult>(
                    new RemoveHashFieldsCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Fields = request.Fields
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
