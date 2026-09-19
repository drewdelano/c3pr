using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace C3PR.Core.Security
{
    public static class RequestSignature
    {
        public const string TimestampHeader = "X-C3PR-Request-Timestamp";
        public const string SignatureHeader = "X-C3PR-Signature";
        public static readonly TimeSpan MaximumAge = TimeSpan.FromMinutes(5);

        public static string Create(
            string secret,
            long unixTimestamp,
            string method,
            string pathAndQuery,
            string body)
        {
            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentException("A signing secret is required.", nameof(secret));
            }

            var canonical = $"v1:{unixTimestamp}:{method?.ToUpperInvariant()}:{pathAndQuery}:{body}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return "v1=" + Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        }

        public static bool IsValid(
            string secret,
            string timestamp,
            string signature,
            string method,
            string pathAndQuery,
            string body,
            DateTimeOffset now)
        {
            if (string.IsNullOrEmpty(secret)
                || !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixTimestamp))
            {
                return false;
            }

            var requestTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            if ((now - requestTime).Duration() > MaximumAge)
            {
                return false;
            }

            return FixedTimeEquals(Create(secret, unixTimestamp, method, pathAndQuery, body), signature);
        }

        internal static bool FixedTimeEquals(string expected, string actual)
        {
            if (string.IsNullOrEmpty(actual))
            {
                return false;
            }

            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var actualBytes = Encoding.UTF8.GetBytes(actual);
            return expectedBytes.Length == actualBytes.Length
                && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
        }
    }
}
