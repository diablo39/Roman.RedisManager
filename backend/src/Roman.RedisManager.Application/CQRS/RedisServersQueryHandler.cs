using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Web.Wolverine
{
    public class RedisServersQuery
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

    }

    public record RedisServerDto(string Name, string Id);

    public record RedisServersQueryResult(
        List<RedisServerDto> Servers,
        int TotalCount,
        int PageNumber,
        int PageSize);

    public static class RedisServersQueryHandler
    {
        public static RedisServersQueryResult Handle(RedisServersQuery query, IRedisServerRepository repository)
        {
            var allServers = repository.ListRedisServers().ToList();

            var totalCount = allServers.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedServers = allServers
                .Skip(skip)
                .Take(query.PageSize)
                .Select(s => new RedisServerDto(s.Name, s.Name.ToMd5Hash()))
                .ToList();

            return new RedisServersQueryResult(pagedServers, totalCount, query.PageNumber, query.PageSize);
        }
    }
}
