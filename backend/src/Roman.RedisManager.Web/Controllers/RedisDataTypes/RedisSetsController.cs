using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS.RedisDataTypes.Set;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Web.Authorization;
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

    [Route("api/redis/data/sets")]
    [ApiController]
    public class RedisSetsController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        /// <summary>
        /// Adds one or more members to a Redis set.
        /// </summary>
        /// <param name="request">The target group, key, members, and optional TTL.</param>
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.Editor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Gets members from a Redis set using cursor-based pagination.
        /// </summary>
        /// <param name="groupId">The Redis server group identifier.</param>
        /// <param name="key">The Redis set key.</param>
        /// <param name="cursor">The scan cursor returned from a previous page.</param>
        /// <param name="pageSize">The maximum number of members to return.</param>
        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        /// <summary>
        /// Removes one or more members from a Redis set.
        /// </summary>
        /// <param name="request">The target group, key, and set members to remove.</param>
        [HttpPost("remove")]
        [Authorize(Policy = AuthorizationPolicies.Editor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
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