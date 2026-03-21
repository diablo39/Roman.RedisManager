using System.Security.Claims;

namespace Roman.RedisManager.Web.Authorization
{
    public class AuthorizationDecisionLogger
    {
        private readonly ILogger<AuthorizationDecisionLogger> _logger;

        public AuthorizationDecisionLogger(ILogger<AuthorizationDecisionLogger> logger)
        {
            _logger = logger;
        }

        public void LogDecision(
            ClaimsPrincipal user,
            string policyName,
            bool allowed)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var roles = string.Join(',', user.FindAll(ClaimTypes.Role).Select(role => role.Value));
            var providerKey = user.FindFirstValue("provider") ?? "unknown";

            _logger.LogInformation(
                "Authorization decision user={UserId} provider={ProviderKey} roles={Roles} policy={PolicyName} allowed={Allowed}",
                userId,
                providerKey,
                roles,
                policyName,
                allowed);
        }
    }
}
