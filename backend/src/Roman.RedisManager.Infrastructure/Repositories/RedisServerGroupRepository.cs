using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisServerGroupRepository : IRedisServerGroupRepository
    {
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisServerGroupRepository(IOptions<RedisConfiguration> redisConfiguration)
        {
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public IReadOnlyCollection<RedisServerGroup> ListRedisServerGroups()
        {
            RedisConfiguration configuration = _redisConfiguration.Value ?? throw new Exception("Configuration can't be null");
            
            var result = configuration.ServerGroups.Select(e => new RedisServerGroup(e.Name, e.Endpoints.ToList())).ToList();

            return result;
        }
    }
}
