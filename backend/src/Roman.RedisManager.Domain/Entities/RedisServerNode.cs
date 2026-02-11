using System;

namespace Roman.RedisManager.Domain.Entities
{
    public class RedisServerNode
    {
        public string Host { get; internal set; }

        public int Port { get; internal set; }

        public string Role { get; internal set; }

        public RedisServerNode(string host, int port, string role)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException("Host cannot be null or whitespace.", nameof(host));
            }

            if (port <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("Role cannot be null or whitespace.", nameof(role));
            }

            Host = host;
            Port = port;
            Role = role.ToLowerInvariant();
        }
    }
}
