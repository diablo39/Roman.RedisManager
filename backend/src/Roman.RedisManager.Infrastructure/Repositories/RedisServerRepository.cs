using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Web.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roman.RedisManager.Infrastructure.Repositories
{
    public class RedisServerRepository : IRedisServerRepository
    {
        private readonly IOptions<RedisConfiguration> _redisConfiguration;

        public RedisServerRepository(IOptions<RedisConfiguration> redisConfiguration)
        {
            _redisConfiguration = redisConfiguration ?? throw new ArgumentNullException(nameof(redisConfiguration));
        }

        public IEnumerable<RedisServer> ListRedisServers()
        {
            RedisConfiguration configuration = _redisConfiguration.Value ?? throw new Exception("Configuration can't be null");
            
            var result = configuration.Servers.Select(e => new RedisServer(e.Name, e.Endpoints.ToList()));

            return result;
        }
    }
}
