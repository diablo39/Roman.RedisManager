using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using RedisKey = Roman.RedisManager.Domain.Entities.RedisKey;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redisConnectionMultiplexer;

        public RedisRepository(IConnectionMultiplexer redisConnectionMultiplexer)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
        }
        public RedisSearchResult SearchForKeys(string predicate)
        {
            var serverEndpoints = _redisConnectionMultiplexer.GetEndPoints();

            //TODO: fix searching - will not work for Redis Cluster
            var server = _redisConnectionMultiplexer.GetServer(serverEndpoints.First());
            var searchResult = server.Keys(pattern: predicate);
            var cursor = (IScanningCursor)searchResult;
            var keys = searchResult.Select(e => new RedisKey(e.ToString())).ToList();

            return new RedisSearchResult(keys, cursor.Cursor);
        }
    }
}
