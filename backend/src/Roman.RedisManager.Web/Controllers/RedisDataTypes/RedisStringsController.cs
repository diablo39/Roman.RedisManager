using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS.RedisDataTypes.String;
using Roman.RedisManager.Domain.Entities.RedisData;
using Roman.RedisManager.Infrastructure.Exceptions;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    public record SetStringRequest(
        Guid GroupId,
        string Key,
        string Value,
        TimeSpan? Ttl,
        SetCondition Condition = SetCondition.None);

    [Route("api/redis/data/strings")]
    [ApiController]
    public class RedisStringsController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SetStringCommandResult>> SetString([FromBody] SetStringRequest request)
        {
            try
            {
                var result = await _bus.InvokeAsync<SetStringCommandResult>(
                    new SetStringCommand
                    {
                        GroupId = request.GroupId,
                        Key = request.Key,
                        Value = request.Value,
                        Ttl = request.Ttl,
                        Condition = request.Condition
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
        public async Task<ActionResult<GetStringQueryResult>> GetString(
            [FromQuery] Guid groupId,
            [FromQuery] string key)
        {
            try
            {
                var result = await _bus.InvokeAsync<GetStringQueryResult>(
                    new GetStringQuery
                    {
                        GroupId = groupId,
                        Key = key
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