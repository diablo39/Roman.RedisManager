using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS;
using Roman.RedisManager.Infrastructure.Exceptions;
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

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RedisKeysSearchQueryResult>> SearchKeys(
            [FromQuery, Required] Guid groupId,
            [FromQuery] string pattern = "*",
            [FromQuery] string cursor = "0",
            [FromQuery] int pageSize = 100)
        {
            try
            {
                var result = await _bus.InvokeAsync<RedisKeysSearchQueryResult>(
                    new RedisKeysSearchQuery
                    {
                        GroupId = groupId,
                        Pattern = pattern,
                        Cursor = cursor,
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{key}/metadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{key}/value")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
