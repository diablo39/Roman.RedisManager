using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Web.Wolverine
{
    public class RedisServerGroupsQuery
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

    }

    public record RedisServerGroupDto(string Name, Guid Id, GroupType GroupType);

    public record RedisServerGroupsQueryResult(
        List<RedisServerGroupDto> ServerGroups,
        int TotalCount,
        int PageNumber,
        int PageSize);

    public static class RedisServerGroupsQueryHandler
    {
        public static RedisServerGroupsQueryResult Handle(RedisServerGroupsQuery query, IRedisServerGroupRepository repository)
        {
            var allServerGroups = repository.ListRedisServerGroups().ToList();

            var totalCount = allServerGroups.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedServerGroups = allServerGroups
                .Skip(skip)
                .Take(query.PageSize)
                .Select(s => new RedisServerGroupDto(s.Name, s.Id, s.GroupType))
                .ToList();

            return new RedisServerGroupsQueryResult(pagedServerGroups, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
