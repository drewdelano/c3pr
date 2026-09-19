using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace C3PR.Core.Security
{
    public static class SlackRequestSignature
    {
        public const string TimestampHeader = "X-Slack-Request-Timestamp";
        public const string SignatureHeader = "X-Slack-Signature";

        public static string Create(string signingSecret, long unixTimestamp, string body)
        {
            if (string.IsNullOrEmpty(signingSecret))
            {
                throw new ArgumentException("A signing secret is required.", nameof(signingSecret));
            }

            var canonical = $"v0:{unixTimestamp}:{body}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingSecret));
            return "v0=" + Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
        }

        public static bool IsValid(
            string signingSecret,
            string timestamp,
            string signature,
            string body,
            DateTimeOffset now)
        {
            if (string.IsNullOrEmpty(signingSecret)
                || !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixTimestamp))
            {
                return false;
            }

            var requestTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            if ((now - requestTime).Duration() > RequestSignature.MaximumAge)
            {
                return false;
            }

            return RequestSignature.FixedTimeEquals(Create(signingSecret, unixTimestamp, body), signature);
        }
    }
}
