using Roman.RedisManager.Domain.Configuration;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Roman.RedisManager.Web.Authorization
{
    public class RoleClaimMappingEvaluator
    {
        private readonly AuthorizationRoleMappingConfiguration _roleConfiguration;

        public RoleClaimMappingEvaluator(IOptions<AuthorizationRoleMappingConfiguration> roleConfiguration)
        {
            _roleConfiguration = roleConfiguration.Value;
        }

        public IReadOnlyCollection<string> ResolveRoles(ClaimsPrincipal principal, string providerKey)
        {
            var resolvedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var mappings = _roleConfiguration.RoleClaimMappings
                .Where(mapping => string.Equals(mapping.ProviderKey, providerKey, StringComparison.OrdinalIgnoreCase));

            foreach (var mapping in mappings)
            {
                var values = principal.FindAll(mapping.ClaimKey)
                    .Select(claim => claim.Value)
                    .ToArray();

                var hasRole = mapping.MatchMode == ClaimMatchMode.All
                    ? mapping.AllowedValues.All(value => values.Contains(value, StringComparer.OrdinalIgnoreCase))
                    : mapping.AllowedValues.Any(value => values.Contains(value, StringComparer.OrdinalIgnoreCase));

                if (hasRole)
                {
                    resolvedRoles.Add(mapping.RoleName);
                }
            }

            return resolvedRoles.ToArray();
        }
    }
}
