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

            // Arrange
            var codec = CreateCodec();
            var envelope = new ContinuationTokenEnvelope
            {
                Version = 1,
                ContextHash = "ctx-hash",
                Mode = ContinuationTokenMode.Standalone,
                StandaloneState = new StandaloneCursorState(42)
            };

            // Act
            var token = codec.Encode(envelope);
            var decoded = codec.Decode(token);

            // Assert
            decoded.Version.ShouldBe((byte)1);
            decoded.ContextHash.ShouldBe("ctx-hash");
            decoded.Mode.ShouldBe(ContinuationTokenMode.Standalone);
            decoded.StandaloneState.ShouldNotBeNull();
            decoded.StandaloneState.Cursor.ShouldBe(42);
        }

        [Fact]
        public void Decode_TamperedToken_ThrowsInvalidContinuationTokenException()
        {

            // Arrange
            var codec = CreateCodec();
            var envelope = new ContinuationTokenEnvelope
            {
                Version = 1,
                ContextHash = "ctx-hash",
                Mode = ContinuationTokenMode.Standalone,
                StandaloneState = new StandaloneCursorState(15)
            };

            // Act
            var token = codec.Encode(envelope);
            var tamperIndex = token.Length / 2;
            var tampered = token[..tamperIndex] + (token[tamperIndex] == 'A' ? 'B' : 'A') + token[(tamperIndex + 1)..];

            // Assert
            var ex = Should.Throw<InvalidContinuationTokenException>(() => codec.Decode(tampered));
            ex.ErrorCode.ShouldBe(ContinuationTokenError.InvalidContinuationToken);
        }

        [Fact]
        public void Decode_UnsupportedVersion_ThrowsInvalidContinuationTokenException()
        {

            // Arrange
            var codec = CreateCodec();
            var envelope = new ContinuationTokenEnvelope
            {
                Version = 2,
                ContextHash = "ctx-hash",
                Mode = ContinuationTokenMode.Standalone,
                StandaloneState = new StandaloneCursorState(15)
            };

            // Act
            var token = codec.Encode(envelope);

            // Assert
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
