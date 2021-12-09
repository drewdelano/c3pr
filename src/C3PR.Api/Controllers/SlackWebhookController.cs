using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SlackNet;
using System;
using C3PR.Core.Services;
using C3PR.Api.Models;

namespace C3PR.Api.Controllers
{
    [Route("SlackWebhook")]
    public class SlackWebhookController : ControllerBase
    {
        readonly ISlackApiClient _slackApiClient;
        readonly ISlackApiDaemonService _slackApiDaemonService;

        public SlackWebhookController(ISlackApiDaemonService slackApiDaemonService, ISlackApiClient slackApiClient)
        {
            _slackApiClient = slackApiClient;
            _slackApiDaemonService = slackApiDaemonService;
        }

        [HttpPost]
        [Route("Event")]
        public async Task<IActionResult> Webhook([FromBody] SlackWebhook webhook)
        {
            if (webhook.Type == "url_verification")
            {
                return Content(webhook.Challenge);
            }

            if (webhook.Type == "event_callback")
            {
                // Ignore retry attempts
                if ((HttpContext.Request.Headers["X-Slack-Retry-Num"].ToString() ?? "").Length > 0)
                {
                    return Ok();
                }

                if (webhook.Event.Type == "message" && webhook.Event.SubType == null)
                {
                    try
                    {
                        await HandleMessage(webhook);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }

            return Ok();
        }

        async Task HandleMessage(SlackWebhook webhook)
        {
            var conversationInfo = await _slackApiClient.Conversations.Info(webhook.Event.Channel);
            var userInfo = await _slackApiClient.Users.Info(webhook.Event.User);

            var text = webhook.Event.Text;
            var channelName = conversationInfo.Name;
            var userName = $"@{userInfo.Name}";

            await _slackApiDaemonService.HandleMessage(text, channelName, userName);
        }
    }

}
