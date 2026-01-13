using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Web.Wolverine
{
    public class RedisServersQueryHandler
    {
        public RedisServersQueryResult Handle(RedisServersQuery query, IRedisServerRepository repository)
        {
            var allServers = repository.ListRedisServers().ToList();

            var totalCount = allServers.Count;
            var skip = (query.PageNumber - 1) * query.PageSize;
            var pagedServers = allServers
                .Skip(skip)
                .Take(query.PageSize)
                .Select(s => new RedisServerDto(s.Name, s.Endpoints.ToList()))
                .ToList();

            return new RedisServersQueryResult(pagedServers, totalCount, query.PageNumber, query.PageSize);
        }
    }

    public record RedisServerDto(string Name, List<string> Endpoints);

    public record RedisServersQueryResult(
        List<RedisServerDto> Servers,
        int TotalCount,
        int PageNumber,
        int PageSize);
}
