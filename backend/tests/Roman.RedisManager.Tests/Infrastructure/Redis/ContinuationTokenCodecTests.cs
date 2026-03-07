using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Infrastructure.Exceptions;
using Roman.RedisManager.Infrastructure.Redis;

namespace Roman.RedisManager.Tests.Infrastructure.Redis
{
    public class ContinuationTokenCodecTests
    {
        [Fact]
        public void EncodeDecode_ValidEnvelope_RoundTrips()
        {
            var codec = CreateCodec();
            var envelope = new ContinuationTokenEnvelope
            {
                Version = 1,
                ContextHash = "ctx-hash",
                Mode = ContinuationTokenMode.Standalone,
                StandaloneState = new StandaloneCursorState(42)
            };

            var token = codec.Encode(envelope);
            var decoded = codec.Decode(token);

            decoded.Version.ShouldBe((byte)1);
            decoded.ContextHash.ShouldBe("ctx-hash");
            decoded.Mode.ShouldBe(ContinuationTokenMode.Standalone);
            decoded.StandaloneState.ShouldNotBeNull();
            decoded.StandaloneState.Cursor.ShouldBe(42);
        }

        [Fact]
        public void Decode_TamperedToken_ThrowsInvalidContinuationTokenException()
        {
            var codec = CreateCodec();
            var envelope = new ContinuationTokenEnvelope
            {
                Version = 1,
                ContextHash = "ctx-hash",
                Mode = ContinuationTokenMode.Standalone,
                StandaloneState = new StandaloneCursorState(15)
            };

            var token = codec.Encode(envelope);
            var tampered = token[..^1] + (token[^1] == 'A' ? 'B' : 'A');

            var ex = Should.Throw<InvalidContinuationTokenException>(() => codec.Decode(tampered));
            ex.ErrorCode.ShouldBe(ContinuationTokenError.InvalidContinuationToken);
        }

        [Fact]
        public void Decode_UnsupportedVersion_ThrowsInvalidContinuationTokenException()
        {
            var codec = CreateCodec();
            var envelope = new ContinuationTokenEnvelope
            {
                Version = 2,
                ContextHash = "ctx-hash",
                Mode = ContinuationTokenMode.Standalone,
                StandaloneState = new StandaloneCursorState(15)
            };

            var token = codec.Encode(envelope);

            var ex = Should.Throw<InvalidContinuationTokenException>(() => codec.Decode(token));
            ex.ErrorCode.ShouldBe(ContinuationTokenError.InvalidContinuationToken);
        }

        private static ContinuationTokenCodec CreateCodec()
        {
            var options = Options.Create(new ContinuationTokenConfiguration
            {
                TokenSecret = "test-secret-key-with-sufficient-length",
                TokenTtlMinutes = 60
            });

            return new ContinuationTokenCodec(options);
        }
    }
}
