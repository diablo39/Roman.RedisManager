namespace Roman.RedisManager.Web.Authorization
{
    public class RouteGroupContextAccessor : IGroupContextAccessor
    {
        public Guid? GetGroupId(HttpContext httpContext)
        {
            if (httpContext.Request.RouteValues.TryGetValue("groupId", out var routeValue) &&
                routeValue is not null &&
                Guid.TryParse(routeValue.ToString(), out var routeGroupId))
            {
                return routeGroupId;
            }

            if (httpContext.Request.Query.TryGetValue("groupId", out var queryValues) &&
                Guid.TryParse(queryValues.ToString(), out var queryGroupId))
            {
                return queryGroupId;
            }

            return null;
        }
    }
}
