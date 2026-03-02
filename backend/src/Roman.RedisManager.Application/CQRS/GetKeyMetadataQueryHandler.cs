using Roman.RedisManager.Domain.Repositories;

namespace Roman.RedisManager.Application.CQRS
{
    public class GetKeyMetadataQuery
    {
        public Guid GroupId { get; set; }
        public string Key { get; set; } = string.Empty;
    }

    public record RedisKeyMetadataDto(string Type, long? TtlMilliseconds);

    public record GetKeyMetadataQueryResult(RedisKeyMetadataDto Metadata);

    public static class GetKeyMetadataQueryHandler
    {
        public static async Task<GetKeyMetadataQueryResult> Handle(
            GetKeyMetadataQuery query,
            IRedisKeyRepository repository)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(repository);

            var metadata = await repository.GetKeyMetadataAsync(query.GroupId, query.Key).ConfigureAwait(false);

            var dto = new RedisKeyMetadataDto(
                metadata.Type.ToString(),
                metadata.Ttl.HasValue ? (long)metadata.Ttl.Value.TotalMilliseconds : null);

            return new GetKeyMetadataQueryResult(dto);
        }
    }
}
