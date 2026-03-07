using Roman.RedisManager.Domain.Entities;

namespace Roman.RedisManager.Infrastructure.Exceptions
{
    public class InvalidContinuationTokenException : ArgumentException
    {
        public InvalidContinuationTokenException(ContinuationTokenError errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public ContinuationTokenError ErrorCode { get; }
    }
}
