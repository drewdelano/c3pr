using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class AddCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;
        public AddCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            return commandContext.Command == ".add" && commandContext.Arguments.Split(' ').Length == 2;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var bits = commandContext.Arguments.Split(' ');
            var targetUser = await _slackApiService.GetUserFromId(bits[0]);

            var specificCarriageNumber = int.Parse(bits[1]);
            if (specificCarriageNumber > train.Carriages.Count)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: The carriage you asked for doesn't exist (it's zero-based, remember?).");
                return;
            }

            if (train.Carriages[specificCarriageNumber].Flairs.Flairs.Contains(CarriageFlair.Lock))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: That carriage is locked, please check with the driver to coordinate shipping or join another carriage.");
                return;
            }

            if (train.Carriages[specificCarriageNumber].Riders.Any(r => r.Name == targetUser))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're already on that carriage.");
                return;
            }

            _carriageManipulationService.AddRiderToCarriageByIndex(train, targetUser, specificCarriageNumber);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());

            var atDriver = await _slackApiService.FormatAtNotificationFromUserName(train.Carriages[specificCarriageNumber].Riders[0].Name);
            var atJoiner = await _slackApiService.FormatAtNotificationFromUserName(targetUser);
            await _slackApiService.PostMessage(commandContext.ChannelName, $"{atDriver}: A new rider has joined the carriage!  {atJoiner} start a thread here to say what you're shipping");
        }
    }
}
