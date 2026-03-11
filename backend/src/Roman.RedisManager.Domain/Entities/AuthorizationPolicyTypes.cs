namespace Roman.RedisManager.Domain.Entities
{
    public enum PermissionAction
    {
        ReadKeys,
        ReadMetadata,
        ReadValues,
        DeleteKey,
        WriteKey
    }

    public enum AuthorizationDecisionReason
    {
        AllowByGroupOverride,
        AllowByGlobalRule,
        DenyNoMatchingRole,
        DenyNoPolicyMatch,
        DenyMissingGroupContext
    }
}
