using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Web.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Roman.RedisManager.Web.Authorization
{
    public class NormalizedRoleClaimsTransformation : IClaimsTransformation
    {
        private readonly OidcProviderProfileResolver _providerResolver;
        private readonly RoleClaimMappingEvaluator _mappingEvaluator;

        public NormalizedRoleClaimsTransformation(
            OidcProviderProfileResolver providerResolver,
            RoleClaimMappingEvaluator mappingEvaluator)
        {
            _providerResolver = providerResolver;
            _mappingEvaluator = mappingEvaluator;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;
            if (identity is null || !identity.IsAuthenticated)
            {
                return Task.FromResult(principal);
            }

            var issuer = identity.FindFirst("iss")?.Value;
            var provider = _providerResolver.ResolveProviderByIssuer(issuer);
            if (provider is null)
            {
                return Task.FromResult(principal);
            }

            var roles = _mappingEvaluator.ResolveRoles(principal, provider.ProviderKey);
            foreach (var role in roles)
            {
                if (!principal.IsInRole(role))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            return Task.FromResult(principal);
        }

    }
}
