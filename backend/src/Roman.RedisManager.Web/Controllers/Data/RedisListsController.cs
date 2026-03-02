using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS.Data;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Infrastructure.Exceptions;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    public record PushToListRequest(
        Guid GroupId,
        string Key,
        IReadOnlyCollection<string> Values,
        ListDirection Direction = ListDirection.Right,
        TimeSpan? Ttl = null);

    public record RemoveFromListRequest(
        Guid GroupId,
        string Key,
        string Value,
        long Count = 0);

    [Route("api/redis/data/lists")]
    [ApiController]
    public class RedisListsController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PushToListCommandResult>> PushToList([FromBody] PushToListRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<PushToListCommandResult>(
                    new PushToListCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Values = request.Values,
                        Direction = request.Direction,
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
        public async Task<ActionResult<GetListRangeQueryResult>> GetListRange(
            [FromQuery] Guid groupId,
            [FromQuery] string key,
            [FromQuery] long start = 0,
            [FromQuery] long stop = -1)
        {
            try
            {
                var result = await _bus.InvokeAsync<GetListRangeQueryResult>(
                    new GetListRangeQuery
                    {
                        GroupId = groupId,
                        Key = key,
                        Start = start,
                        Stop = stop
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
        public async Task<ActionResult<RemoveFromListCommandResult>> RemoveFromList([FromBody] RemoveFromListRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<RemoveFromListCommandResult>(
                    new RemoveFromListCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Value = request.Value,
                        Count = request.Count
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
