using Roman.RedisManager.Domain.Entities;
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
            PermissionAction action,
            Guid? groupId,
            bool allowed,
            AuthorizationDecisionReason reason)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var roles = string.Join(',', user.FindAll(ClaimTypes.Role).Select(role => role.Value));
            var providerKey = user.FindFirstValue("provider") ?? "unknown";

            _logger.LogInformation(
                "Authorization decision user={UserId} provider={ProviderKey} roles={Roles} action={Action} groupId={GroupId} allowed={Allowed} reason={Reason}",
                userId,
                providerKey,
                roles,
                action,
                groupId,
                allowed,
                reason);
        }
    }
}
