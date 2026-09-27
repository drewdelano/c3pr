using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using C3PR.Api.Security;
using C3PR.Core.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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

        [Test]
        public async Task SlackMiddlewareVerifiesAndRewindsTheExactUtf8RequestBody()
        {
            const string secret = "slack-signing-secret";
            const string body = "{\n  \"token\": \"legacy-token\",\n  \"challenge\": \"challenge-✓\",\n  \"type\": \"url_verification\"\n}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string downstreamBody = null;
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["SlackSigningSecret"] = secret,
                })
                .Build();
            var middleware = new RequestAuthenticationMiddleware(
                async context =>
                {
                    using var reader = new StreamReader(
                        context.Request.Body,
                        Encoding.UTF8,
                        false,
                        1024,
                        leaveOpen: true);
                    downstreamBody = await reader.ReadToEndAsync();
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                },
                configuration);
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/SlackWebhook/Event";
            context.Request.ContentType = "application/json";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            context.Request.Headers[SlackRequestSignature.TimestampHeader] = timestamp.ToString();
            context.Request.Headers[SlackRequestSignature.SignatureHeader] =
                SlackRequestSignature.Create(secret, timestamp, body);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
            Assert.That(downstreamBody, Is.EqualTo(body));
        }

        [Test]
        public async Task C3prMiddlewareExcludesApiGatewayStageFromTheSignedApplicationPath()
        {
            const string secret = "callback-secret";
            const string body = "channelName=ship-it&shipUrl=https%3A%2F%2Fexample.com";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["C3prCallbackSecret"] = secret,
                })
                .Build();
            var middleware = new RequestAuthenticationMiddleware(
                context =>
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    return Task.CompletedTask;
                },
                configuration);
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.PathBase = "/Prod";
            context.Request.Path = "/Shipping/SetShipUrl";
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            context.Request.Headers[RequestSignature.TimestampHeader] = timestamp.ToString();
            context.Request.Headers[RequestSignature.SignatureHeader] = RequestSignature.Create(
                secret, timestamp, "POST", "/Shipping/SetShipUrl", body);

            await middleware.InvokeAsync(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status204NoContent));
        }
    }
}
