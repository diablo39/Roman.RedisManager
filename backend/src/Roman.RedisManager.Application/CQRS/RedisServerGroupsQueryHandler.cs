using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities.Server;

namespace Roman.RedisManager.Application.CQRS
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
        public static RedisServerGroupsQueryResult Handle(RedisServerGroupsQuery query, IOptions<RedisConfiguration> configuration)
        {
            var allServerGroups = configuration.Value.ServerGroups
                .Select(e => new RedisServerGroupDto(e.Name, e.Id, e.GroupType))
                .ToList();

            var totalCount = allServerGroups.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedServerGroups = allServerGroups
                .Skip(skip)
                .Take(query.PageSize)
                .ToList();

            return new RedisServerGroupsQueryResult(pagedServerGroups, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
