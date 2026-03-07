namespace Roman.RedisManager.Domain.Entities
{
    public enum ContinuationTokenError
    {
        InvalidContinuationToken,
        ContinuationContextMismatch,
        ContinuationNotResumable
    }
}
