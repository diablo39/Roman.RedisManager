using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Infrastructure.Exceptions;

namespace Roman.RedisManager.Web.ProblemDetails
{
    internal static class ContinuationTokenProblemDetailsMapper
    {
        public static string ToCode(ContinuationTokenError error)
        {
            return error switch
            {
                ContinuationTokenError.InvalidContinuationToken => "invalid_continuation_token",
                ContinuationTokenError.ContinuationContextMismatch => "continuation_context_mismatch",
                ContinuationTokenError.ContinuationNotResumable => "continuation_not_resumable",
                _ => "invalid_continuation_token"
            };
        }

        public static string ToType(ContinuationTokenError error)
        {
            return $"https://roman.redismanager/errors/{ToCode(error)}";
        }
    }
}
