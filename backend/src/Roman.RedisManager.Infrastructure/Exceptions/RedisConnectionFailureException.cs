using System;

namespace Roman.RedisManager.Infrastructure.Exceptions
{
    public class RedisConnectionFailureException(string message, Exception innerException) : Exception(message, innerException)
    {
    }
}
