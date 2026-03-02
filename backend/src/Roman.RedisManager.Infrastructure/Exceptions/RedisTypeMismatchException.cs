namespace Roman.RedisManager.Infrastructure.Exceptions
{
    public class RedisTypeMismatchException(string message, Exception innerException) : Exception(message, innerException)
    {
    }
}
