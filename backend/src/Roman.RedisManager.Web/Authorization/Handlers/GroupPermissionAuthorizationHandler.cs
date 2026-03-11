using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Web.Authorization.Requirements;
using System.Security.Claims;

namespace Roman.RedisManager.Web.Authorization.Handlers
{
    public class GroupPermissionAuthorizationHandler : AuthorizationHandler<GroupPermissionRequirement>
    {
        private readonly AuthorizationPermissionConfiguration _permissions;
        private readonly RedisConfiguration _redisConfiguration;
        private readonly IGroupContextAccessor _groupContextAccessor;
        private readonly AuthorizationDecisionLogger _decisionLogger;

        public GroupPermissionAuthorizationHandler(
            IOptions<AuthorizationPermissionConfiguration> permissions,
            IOptions<RedisConfiguration> redisConfiguration,
            IGroupContextAccessor groupContextAccessor,
            AuthorizationDecisionLogger decisionLogger)
        {
            _permissions = permissions.Value;
            _redisConfiguration = redisConfiguration.Value;
            _groupContextAccessor = groupContextAccessor;
            _decisionLogger = decisionLogger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupPermissionRequirement requirement)
        {
            if (context.Resource is not HttpContext httpContext)
            {
                return Task.CompletedTask;
            }

            var groupId = _groupContextAccessor.GetGroupId(httpContext);
            if (!groupId.HasValue)
            {
                _decisionLogger.LogDecision(context.User, requirement.Action, null, false, AuthorizationDecisionReason.DenyMissingGroupContext);
                return Task.CompletedTask;
            }

            var roles = context.User.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var serverGroup = _redisConfiguration.ServerGroups
                .FirstOrDefault(group => group.Id == groupId.Value);

            var overrideRule = serverGroup?.AuthorizationOverrides?.Rules
                .FirstOrDefault(rule => rule.Action == requirement.Action);

            if (overrideRule is not null)
            {
                var allowedByOverride = overrideRule.AllowedRoles.Any(role => roles.Contains(role));
                _decisionLogger.LogDecision(
                    context.User,
                    requirement.Action,
                    groupId,
                    allowedByOverride,
                    allowedByOverride ? AuthorizationDecisionReason.AllowByGroupOverride : AuthorizationDecisionReason.DenyNoMatchingRole);

                if (allowedByOverride)
                {
                    context.Succeed(requirement);
                }

                return Task.CompletedTask;
            }

            var globalRule = _permissions.Global.FirstOrDefault(rule => rule.Action == requirement.Action);
            var allowedByGlobal = globalRule is not null && globalRule.AllowedRoles.Any(role => roles.Contains(role));
            _decisionLogger.LogDecision(
                context.User,
                requirement.Action,
                groupId,
                allowedByGlobal,
                allowedByGlobal ? AuthorizationDecisionReason.AllowByGlobalRule : AuthorizationDecisionReason.DenyNoPolicyMatch);

            if (allowedByGlobal)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
