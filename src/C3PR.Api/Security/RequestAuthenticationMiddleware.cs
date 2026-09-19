using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using C3PR.Core.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace C3PR.Api.Security
{
    public class RequestAuthenticationMiddleware
    {
        readonly RequestDelegate _next;
        readonly string _callbackSecret;
        readonly string _slackSigningSecret;

        public RequestAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _callbackSecret = configuration["C3prCallbackSecret"];
            _slackSigningSecret = configuration["SlackSigningSecret"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Shipping", StringComparison.OrdinalIgnoreCase))
            {
                await AuthenticateC3prCallback(context);
                return;
            }

            if (context.Request.Path.Equals("/SlackWebhook/Event", StringComparison.OrdinalIgnoreCase))
            {
                await AuthenticateSlackRequest(context);
                return;
            }

            await _next(context);
        }

        async Task AuthenticateC3prCallback(HttpContext context)
        {
            if (string.IsNullOrEmpty(_callbackSecret))
            {
                await Reject(context, StatusCodes.Status503ServiceUnavailable, "C3PR callback authentication is not configured.");
                return;
            }

            var body = await ReadBody(context.Request);
            var pathAndQuery = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
            if (!RequestSignature.IsValid(
                _callbackSecret,
                context.Request.Headers[RequestSignature.TimestampHeader],
                context.Request.Headers[RequestSignature.SignatureHeader],
                context.Request.Method,
                pathAndQuery,
                body,
                DateTimeOffset.UtcNow))
            {
                await Reject(context, StatusCodes.Status401Unauthorized, "Invalid C3PR request signature.");
                return;
            }

            await _next(context);
        }

        async Task AuthenticateSlackRequest(HttpContext context)
        {
            if (string.IsNullOrEmpty(_slackSigningSecret))
            {
                await Reject(context, StatusCodes.Status503ServiceUnavailable, "Slack request authentication is not configured.");
                return;
            }

            var body = await ReadBody(context.Request);
            if (!SlackRequestSignature.IsValid(
                _slackSigningSecret,
                context.Request.Headers[SlackRequestSignature.TimestampHeader],
                context.Request.Headers[SlackRequestSignature.SignatureHeader],
                body,
                DateTimeOffset.UtcNow))
            {
                await Reject(context, StatusCodes.Status401Unauthorized, "Invalid Slack request signature.");
                return;
            }

            await _next(context);
        }

        static async Task<string> ReadBody(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, false, 1024, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        static async Task Reject(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(message);
        }
    }
}
