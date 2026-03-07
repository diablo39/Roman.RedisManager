using Microsoft.Extensions.Options;
using Roman.RedisManager.Domain.Configuration;
using Roman.RedisManager.Domain.Entities;
using Roman.RedisManager.Domain.Repositories;
using Roman.RedisManager.Infrastructure.Exceptions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Roman.RedisManager.Infrastructure.Redis
{
    public class ContinuationTokenCodec : IContinuationTokenCodec
    {
        private const int _signatureLength = 32;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly byte[] _secretKey;

        public ContinuationTokenCodec(IOptions<ContinuationTokenConfiguration> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            var secret = options.Value.TokenSecret;
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("ContinuationToken.TokenSecret must be configured.");
            }

            _secretKey = Encoding.UTF8.GetBytes(secret);
        }

        public string Encode(ContinuationTokenEnvelope envelope)
        {
            ArgumentNullException.ThrowIfNull(envelope);

            var payload = JsonSerializer.SerializeToUtf8Bytes(envelope, _jsonOptions);
            var signature = ComputeSignature(payload);

            var signedPayload = new byte[payload.Length + signature.Length];
            Buffer.BlockCopy(payload, 0, signedPayload, 0, payload.Length);
            Buffer.BlockCopy(signature, 0, signedPayload, payload.Length, signature.Length);

            return Base64UrlEncode(signedPayload);
        }

        public ContinuationTokenEnvelope Decode(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            byte[] signedPayload;
            try
            {
                signedPayload = Base64UrlDecode(token);
            }
            catch (FormatException)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            if (signedPayload.Length <= _signatureLength)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            var payloadLength = signedPayload.Length - _signatureLength;
            var payload = new byte[payloadLength];
            var providedSignature = new byte[_signatureLength];

            Buffer.BlockCopy(signedPayload, 0, payload, 0, payloadLength);
            Buffer.BlockCopy(signedPayload, payloadLength, providedSignature, 0, _signatureLength);

            var expectedSignature = ComputeSignature(payload);
            if (!CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature))
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            ContinuationTokenEnvelope? envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<ContinuationTokenEnvelope>(payload, _jsonOptions);
            }
            catch (JsonException)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            if (envelope is null || envelope.Version != 1)
            {
                throw new InvalidContinuationTokenException(
                    ContinuationTokenError.InvalidContinuationToken,
                    "Continuation token is invalid. Start a new search.");
            }

            return envelope;
        }

        private byte[] ComputeSignature(byte[] payload)
        {
            using var hmac = new HMACSHA256(_secretKey);
            return hmac.ComputeHash(payload);
        }

        private static string Base64UrlEncode(byte[] payload)
        {
            return Convert.ToBase64String(payload)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string token)
        {
            var normalized = token.Replace('-', '+').Replace('_', '/');
            switch (normalized.Length % 4)
            {
                case 2:
                    normalized += "==";
                    break;
                case 3:
                    normalized += "=";
                    break;
            }

            return Convert.FromBase64String(normalized);
        }
    }
}
