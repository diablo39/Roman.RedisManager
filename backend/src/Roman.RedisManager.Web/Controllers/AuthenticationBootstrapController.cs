using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roman.RedisManager.Application.CQRS;
using Wolverine;

namespace Roman.RedisManager.Web.Controllers
{
    /// <summary>
    /// Exposes browser-safe authentication bootstrap settings for frontend clients.
    /// </summary>
    /// <remarks>
    /// Route base: <c>api/authentication</c>.
    ///
    /// This endpoint is anonymous by design so a frontend can request sign-in configuration
    /// before obtaining an access token.
    /// </remarks>
    [Route("api/authentication")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationBootstrapController : ControllerBase
    {
        private readonly IMessageBus _bus;

        public AuthenticationBootstrapController(IMessageBus bus) => _bus = bus;

        /// <summary>
        /// Gets authentication bootstrap configuration for SPA startup.
        /// </summary>
        /// <remarks>
        /// Returns enabled and sign-in-capable providers with browser-safe OIDC configuration.
        ///
        /// Example request:
        /// <code>GET /api/authentication/bootstrap</code>
        ///
        /// Response semantics:
        /// - <c>bootstrapState = available</c>: at least one provider can be used for interactive sign-in.
        /// - <c>bootstrapState = unavailable</c>: no provider is currently sign-in-capable; see <c>unavailableReasonCodes</c>.
        /// </remarks>
        [HttpGet("bootstrap")]
        [EndpointSummary("Get SPA authentication bootstrap settings")]
        [EndpointDescription("Returns enabled sign-in providers and browser-safe OIDC settings used by frontend clients to initiate login.")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthenticationBootstrapQueryResult), StatusCodes.Status200OK)]
        public async Task<AuthenticationBootstrapQueryResult> GetBootstrap()
        {
            return await _bus.InvokeAsync<AuthenticationBootstrapQueryResult>(new AuthenticationBootstrapQuery());
        }
    }
}
