using Microsoft.AspNetCore.Authorization;
using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Web.Authorization.Requirements
{
    public class GroupPermissionRequirement : IAuthorizationRequirement
    {
        public GroupPermissionRequirement(PermissionAction action)
        {
            Action = action;
        }

        public PermissionAction Action { get; }
    }
}
