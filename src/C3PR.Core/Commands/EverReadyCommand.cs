using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class EverReadyCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;

        public EverReadyCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            return commandContext.Command == ".everready" && commandContext.Arguments == "";
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var rider = train.Carriages[0].Riders.FirstOrDefault(r => r.Name == commandContext.UserName);
            if (rider == null)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're not on this carriage.");
                return;
            }

            if (rider.Flairs.Flairs.Contains(RiderFlair.EverReady))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're already everyready.");
                return;
            }

            if (_carriageManipulationService.IsPhaseUnknown(train.Phase))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Uh, I don't know what phase this is...Can you fix it?");
                return;
            }

            if (train.Carriages[0].Riders.IndexOf(rider) == 0)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: The driver can't everready -- they're in charge of making sure everything is working");
                return;
            }

            if (_carriageManipulationService.IsNotHeld(train, rider))
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Train is held!  Can't continue.");
                return;
            }

            // everready replaces ready
            if (rider.Flairs.Flairs.Contains(RiderFlair.Ready))
            {
                rider.Flairs.Flairs.Remove(RiderFlair.Ready);
            }
            rider.Flairs.Flairs.Add(RiderFlair.EverReady);
            await _carriageManipulationService.AdvanceCarriageIfNecessary(train, commandContext.ChannelName);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }
    }
}
