using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class ReadyCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;
        public ReadyCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".ready" && commandContext.Arguments == "")
            {
                return true;
            }

            if (commandContext.Command == ".ready" && commandContext.Arguments.Length > 1 && commandContext.Arguments.Split(' ').Length == 1)
            {
                return true;
            }

            return false;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".ready" && commandContext.Arguments == "")
            {
                await Simple(commandContext);
            }

            if (commandContext.Command == ".ready" && commandContext.Arguments.Length > 1 && commandContext.Arguments.Split(' ').Length == 1)
            {
                await Specific(commandContext);
            }
        }

        async Task Specific(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var targetUser = await _slackApiService.GetUserFromId(commandContext.Arguments);

            if (train.IsEmpty)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: {targetUser} isn't on the train.");
                return;
            }

            var rider = train.Carriages[0].Riders.FirstOrDefault(r => r.Name == targetUser);
            if (rider == null)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: {targetUser} isn't up yet.");
                return;
            }

            if (rider.Flairs.Flairs.Contains(RiderFlair.Ready) || rider.Flairs.Flairs.Contains(RiderFlair.EverReady))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: {targetUser} is already ready.");
                return;
            }

            if (_carriageManipulationService.IsPhaseUnknown(train.Phase))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Uh, I don't know what phase this is...Can you fix it?");
                return;
            }

            if (_carriageManipulationService.IsNotHeld(train, rider))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Train is held!  Can't continue.");
                return;
            }

            rider.Flairs.Flairs.Add(RiderFlair.Ready);
            await _carriageManipulationService.AdvanceCarriageIfNecessary(train, commandContext.ChannelName);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }

        async Task Simple(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            if (train.IsEmpty)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're not on the train.");
                return;
            }

            var rider = train.Carriages[0].Riders.FirstOrDefault(r => r.Name == commandContext.UserName);
            if (rider == null)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're not up yet.");
                return;
            }

            if (rider.Flairs.Flairs.Contains(RiderFlair.Ready) || rider.Flairs.Flairs.Contains(RiderFlair.EverReady))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're already ready.");
                return;
            }

            if (_carriageManipulationService.IsPhaseUnknown(train.Phase))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Uh, I don't know what phase this is...Can you fix it?");
                return;
            }

            if (_carriageManipulationService.IsNotHeld(train, rider))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Train is held!  Can't continue.");
                return;
            }

            rider.Flairs.Flairs.Add(RiderFlair.Ready);
            await _carriageManipulationService.AdvanceCarriageIfNecessary(train, commandContext.ChannelName);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }
    }
}
