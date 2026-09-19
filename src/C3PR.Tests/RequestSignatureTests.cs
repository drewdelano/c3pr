using System;
using C3PR.Core.Security;
using NUnit.Framework;

namespace C3PR.Tests
{
    public class RequestSignatureTests
    {
        static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(1_700_000_000);

        [Test]
        public void C3prSignatureAcceptsAnUnmodifiedRecentRequest()
        {
            var timestamp = Now.ToUnixTimeSeconds();
            var signature = RequestSignature.Create("callback-secret", timestamp, "POST", "/Shipping/SetShipUrl", "channelName=ship-it&shipUrl=https%3A%2F%2Fexample.com");

            Assert.That(RequestSignature.IsValid(
                "callback-secret", timestamp.ToString(), signature, "POST", "/Shipping/SetShipUrl",
                "channelName=ship-it&shipUrl=https%3A%2F%2Fexample.com", Now), Is.True);
        }

        [TestCase("wrong-secret", "POST", "/Shipping/SetShipUrl", "channelName=ship-it")]
        [TestCase("callback-secret", "GET", "/Shipping/SetShipUrl", "channelName=ship-it")]
        [TestCase("callback-secret", "POST", "/Shipping/Other", "channelName=ship-it")]
        [TestCase("callback-secret", "POST", "/Shipping/SetShipUrl", "channelName=other")]
        public void C3prSignatureRejectsTampering(string secret, string method, string path, string body)
        {
            var timestamp = Now.ToUnixTimeSeconds();
            var signature = RequestSignature.Create("callback-secret", timestamp, "POST", "/Shipping/SetShipUrl", "channelName=ship-it");

            Assert.That(RequestSignature.IsValid(secret, timestamp.ToString(), signature, method, path, body, Now), Is.False);
        }

        [Test]
        public void C3prSignatureRejectsReplaysOlderThanFiveMinutes()
        {
            var timestamp = Now.AddMinutes(-6).ToUnixTimeSeconds();
            var signature = RequestSignature.Create("callback-secret", timestamp, "GET", "/Shipping/SafeToDeployProd?channelName=ship-it", "");

            Assert.That(RequestSignature.IsValid(
                "callback-secret", timestamp.ToString(), signature, "GET",
                "/Shipping/SafeToDeployProd?channelName=ship-it", "", Now), Is.False);
        }

        [Test]
        public void SlackSignatureMatchesSlacksPublishedAlgorithm()
        {
            const string body = "token=example&team_id=T123";
            var timestamp = Now.ToUnixTimeSeconds();
            var signature = SlackRequestSignature.Create("slack-secret", timestamp, body);

            Assert.That(SlackRequestSignature.IsValid(
                "slack-secret", timestamp.ToString(), signature, body, Now), Is.True);
            Assert.That(SlackRequestSignature.IsValid(
                "slack-secret", timestamp.ToString(), signature, body + "tampered", Now), Is.False);
        }
    }
}
