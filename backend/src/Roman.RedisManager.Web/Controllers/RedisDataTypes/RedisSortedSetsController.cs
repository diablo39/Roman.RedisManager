using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS.RedisDataTypes;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Infrastructure.Exceptions;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    public record AddToSortedSetRequest(
        Guid GroupId,
        string Key,
        IReadOnlyCollection<RedisSortedSetEntry> Entries,
        TimeSpan? Ttl = null);

    public record RemoveFromSortedSetRequest(
        Guid GroupId,
        string Key,
        IReadOnlyCollection<string> Members);

    [Route("api/redis/data/sorted-sets")]
    [ApiController]
    public class RedisSortedSetsController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AddToSortedSetCommandResult>> AddToSortedSet([FromBody] AddToSortedSetRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<AddToSortedSetCommandResult>(
                    new AddToSortedSetCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Entries = request.Entries,
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
        public async Task<ActionResult<GetSortedSetRangeQueryResult>> GetSortedSetRange(
            [FromQuery] Guid groupId,
            [FromQuery] string key,
            [FromQuery] long start = 0,
            [FromQuery] long stop = -1)
        {
            try
            {
                var result = await _bus.InvokeAsync<GetSortedSetRangeQueryResult>(
                    new GetSortedSetRangeQuery
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
        public async Task<ActionResult<RemoveFromSortedSetCommandResult>> RemoveFromSortedSet([FromBody] RemoveFromSortedSetRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<RemoveFromSortedSetCommandResult>(
                    new RemoveFromSortedSetCommand
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
