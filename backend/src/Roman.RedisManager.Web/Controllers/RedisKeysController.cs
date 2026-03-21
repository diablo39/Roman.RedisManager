using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Web.Authorization;
using Roman.RedisManager.Web.ProblemDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    [Route("api/redis-keys")]
    [ApiController]
    public class RedisKeysController(IMessageBus bus) : ControllerBase
    {
        private readonly IMessageBus _bus = bus;

        /// <summary>
        /// Searches keys in a Redis server group using a glob pattern and continuation token paging.
        /// </summary>
        /// <param name="groupId">The Redis server group identifier.</param>
        /// <param name="pattern">The Redis key pattern, for example * or user:*.</param>
        /// <param name="continuationToken">An optional opaque token returned by a previous search page.</param>
        /// <param name="pageSize">The maximum number of keys to return in the page.</param>
        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RedisKeysSearchQueryResult>> SearchKeys(
            [FromQuery, Required] Guid groupId,
            [FromQuery] string pattern = "*",
            [FromQuery] string? continuationToken = null,
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var result = await _bus.InvokeAsync<RedisKeysSearchQueryResult>(
                    new RedisKeysSearchQuery
                    {
                        GroupId = groupId,
                        Pattern = pattern,
                        ContinuationToken = continuationToken,
                        PageSize = pageSize
                    });

                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (RedisConnectionFailureException ex)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: ex.Message);
            }
            catch (InvalidContinuationTokenException ex)
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<RedisKeysController>>();
                var errorCode = ContinuationTokenProblemDetailsMapper.ToCode(ex.ErrorCode);
                logger.LogWarning(
                    "Rejected continuation token with error code {ErrorCode} on {RequestPath}",
                    errorCode,
                    HttpContext.Request.Path.ToString());

                var details = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid continuation token",
                    Detail = ex.Message,
                    Type = ContinuationTokenProblemDetailsMapper.ToType(ex.ErrorCode)
                };

                details.Extensions["code"] = errorCode;
                details.Extensions["traceId"] = HttpContext.TraceIdentifier;
                details.Extensions["requestPath"] = HttpContext.Request.Path.ToString();

                return new ObjectResult(details)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        /// <summary>
        /// Deletes a key from a Redis server group.
        /// </summary>
        /// <param name="key">The Redis key to delete.</param>
        /// <param name="groupId">The Redis server group identifier.</param>
        [HttpDelete("{key}")]
        [Authorize(Policy = AuthorizationPolicies.Editor)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteKeyCommandResult>> DeleteKey(
            string key,
            [FromQuery] Guid groupId)
        {
            try
            {
                var result = await _bus.InvokeAsync<DeleteKeyCommandResult>(
                    new DeleteKeyCommand
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
            catch (RedisConnectionFailureException ex)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
            }
        }

        /// <summary>
        /// Retrieves metadata for a Redis key.
        /// </summary>
        /// <param name="key">The Redis key to inspect.</param>
        /// <param name="groupId">The Redis server group identifier.</param>
        [HttpGet("{key}/metadata")]
        [Authorize(Policy = AuthorizationPolicies.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetKeyMetadataQueryResult>> GetKeyMetadata(
            string key,
            [FromQuery] Guid groupId)
        {
            try
            {
                var result = await _bus.InvokeAsync<GetKeyMetadataQueryResult>(
                    new GetKeyMetadataQuery
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
            catch (RedisConnectionFailureException ex)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the value payload for a Redis key.
        /// </summary>
        /// <param name="key">The Redis key to read.</param>
        /// <param name="groupId">The Redis server group identifier.</param>
        [HttpGet("{key}/value")]
        [Authorize(Policy = AuthorizationPolicies.Reader)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetKeyValueQueryResult>> GetKeyValue(
            string key,
            [FromQuery] Guid groupId)
        {
            try
            {
                var result = await _bus.InvokeAsync<GetKeyValueQueryResult>(
                    new GetKeyValueQuery
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
            catch (RedisConnectionFailureException ex)
            {
                return Problem(statusCode: StatusCodes.Status500InternalServerError, detail: ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: ex.Message);
            }
        }
    }
}
