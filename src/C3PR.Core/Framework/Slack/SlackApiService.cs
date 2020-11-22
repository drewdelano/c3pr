using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Commands;
using SlackNet;
using SlackNet.Bot;
using SlackNet.WebApi;

namespace C3PR.Core.Framework
{
    public class SlackApiService : ISlackApiService
    {
        readonly ISlackApiClient _slackApiClient;

        public SlackApiService(ISlackApiClient slackApiClient)
        {
            _slackApiClient = slackApiClient;
        }

        public async Task<string> GetChannelTopic(string channelName)
        {
            var channel = await GetConversationByName(channelName);
            var topicText = channel.Topic.Value;

            topicText = topicText.Replace((char)160, ' ');
            topicText = topicText.Replace("&lt;", "<");
            topicText = topicText.Replace("&gt;", ">");

            return topicText;
        }

        async Task<Conversation> GetConversationByName(string channelName)
        {
            var channels = await _slackApiClient.Conversations.List();
            var channel = channels.Channels.First(c => c.Name == channelName);
            return channel;
        }

        public async Task SetChannelTopic(string channelName, string newTopic)
        {
            var channel = await GetConversationByName(channelName);


            await _slackApiClient.Conversations.SetTopic(channel.Id, newTopic);
        }

        public async Task PostMessage(string channelName, string message)
        {
            var channel = await GetConversationByName(channelName);

            await _slackApiClient.Chat.PostMessage(new Message
            {
                Channel = channel.Id,
                Text = message
            });
        }

        public async Task<string> GetUserFromId(string userId)
        {
            var user = await _slackApiClient.Users.Info(userId.TrimStart('<', '@').TrimEnd('>'));
            return $"@{user?.Name}";
        }

        public async Task<bool> IsChannelNameValid(string channelName)
        {
            var channels = await _slackApiClient.Conversations.List();
            var channel = channels.Channels.FirstOrDefault(c => c.Name == channelName);

            return channel != null;
        }
    }
}
