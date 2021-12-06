using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Framework.Slack;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class TriggerCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly IExternalBuildTrigger _externalBuildTrigger;

        public TriggerCommand(ISlackApiService slackApiService, IExternalBuildTrigger externalBuildTrigger)
        {
            _slackApiService = slackApiService;
            _externalBuildTrigger = externalBuildTrigger;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".trigger")
            {
                return true;
            }

            return false;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            var channelName = commandContext.ChannelName;
            var topic = await _slackApiService.GetChannelTopic(channelName);
            var train = Train.Parse(topic);
            
            await _slackApiService.PostMessage(commandContext.ChannelName, $"Triggering the build pipeline...");
            
            var messageToSelf = await _slackApiService.ReadLatestMessageToSelf();
            var store = SlackMessageStorage.Parse(messageToSelf);

            var channel = store.FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
            {
                channel = new SlackMessageStorage
                {
                    ChannelName = channelName,
                    ShipUrl = ""
                };

                messageToSelf = SlackMessageStorage.Stringify(store);
                await _slackApiService.PostMessage("@slackbot", messageToSelf);
            }
            try
            {
                await _externalBuildTrigger.TriggerBuild(channel);
            }
            catch (Exception ex)
            {
                var atDriver = await _slackApiService.FormatAtNotificationFromUserName(train.Carriages[0].Riders[0].Name);
                await _slackApiService.PostMessage(commandContext.ChannelName, $"Something went wrong with the build pipeline {atDriver}");

                Console.WriteLine(ex.ToString());
            }
        }
    }
}
