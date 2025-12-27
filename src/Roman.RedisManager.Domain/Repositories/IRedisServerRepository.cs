using Roman.RedisManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roman.RedisManager.Domain.Repositories
{
    public interface IRedisServerRepository
    {
        IEnumerable<RedisServer> ListRedisServers();
    }
}
