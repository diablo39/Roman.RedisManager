namespace Roman.RedisManager.Web.Authorization
{
    public interface IGroupContextAccessor
    {
        Guid? GetGroupId(HttpContext httpContext);
    }
}
