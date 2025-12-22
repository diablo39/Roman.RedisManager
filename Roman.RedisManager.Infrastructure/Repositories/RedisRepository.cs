using Roman.RedisManager.Domain.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IConnectionMultiplexer _redisConnectionMultiplexer;

        public RedisRepository(IConnectionMultiplexer redisConnectionMultiplexer)
        {
            _redisConnectionMultiplexer = redisConnectionMultiplexer;
        }
        public ReadisSearchResult SearchForKeys(string predicate)
        {            
            throw new NotImplementedException();
        }
    }
}
