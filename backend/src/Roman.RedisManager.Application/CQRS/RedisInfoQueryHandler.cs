using System;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roman.RedisManager.Application.CQRS
{
    public class RedisInfoQuery
    {
        public Guid GroupId { get; set; }
        public string Host { get; set; } = default!;
        public int Port { get; set; }
    }

    public record RedisInfoQueryResult(IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Sections);

    public static class RedisInfoQueryHandler
    {
        public static async Task<RedisInfoQueryResult> Handle(RedisInfoQuery query, IRedisRepository repository)
        {
            var info = await repository.GetInfoAsync(query.GroupId, query.Host, query.Port);
            return new RedisInfoQueryResult(info.Sections);
        }
    }
}
