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
    public class BuildCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly IExternalBuildTrigger _externalBuildTrigger;

        public BuildCommand(ISlackApiService slackApiService, IExternalBuildTrigger externalBuildTrigger)
        {
            _slackApiService = slackApiService;
            _externalBuildTrigger = externalBuildTrigger;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".build")
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
            
            await _slackApiService.PostMessage(commandContext.ChannelName, $"Running the build pipeline...");
            
            // The GitHub Actions trigger is configured from Lambda environment
            // variables and does not consume legacy Slackbot-DM storage. Reading
            // that DM here prevents .build from dispatching when Slack refuses
            // bot-to-Slackbot history access, so pass only the active channel.
            var channel = new SlackMessageStorage
            {
                ChannelName = channelName,
                ShipUrl = ""
            };
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
