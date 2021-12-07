using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class JoinCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;
        public JoinCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".join" && commandContext.Arguments == "")
            {
                return true;
            }

            if (commandContext.Command == ".join" && int.TryParse(commandContext.Arguments, out var num) && num >= 0)
            {
                return true;
            }

            return false;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".join" && commandContext.Arguments == "")
            {
                await Simple(commandContext);
            }

            if (commandContext.Command == ".join" && int.TryParse(commandContext.Arguments, out var num) && num >= 0)
            {
                await Specfic(commandContext);
            }
        }

        async Task Specfic(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var specificCarriageNumber = int.Parse(commandContext.Arguments);
            if (specificCarriageNumber >= train.Carriages.Count)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Only {train.Carriages.Count} carriages are available and are zero-based.");
                return;
            }

            if (train.Carriages[specificCarriageNumber].Flairs.Flairs.Contains(CarriageFlair.Lock))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: That carriage is locked, please check with the driver to coordinate shipping or join another carriage.");
                return;
            }

            if (train.Carriages[specificCarriageNumber].Riders.Any(r => r.Name == commandContext.UserName))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're already on that carriage.");
                return;
            }

            _carriageManipulationService.AddRiderToCarriageByIndex(train, commandContext.UserName, specificCarriageNumber);
            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());

            var atDriver = await _slackApiService.FormatAtNotificationFromUserName(train.Carriages[specificCarriageNumber].Riders[0].Name);
            var atJoiner = await _slackApiService.FormatAtNotificationFromUserName(commandContext.UserName);
            await _slackApiService.PostMessage(commandContext.ChannelName, $"{atDriver}: A new rider has joined the carriage!  {atJoiner} start a thread here to say what you're shipping");
        }

        async Task Simple(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            _carriageManipulationService.AddRiderInNewCarriage(train, commandContext.UserName);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }
    }
}
