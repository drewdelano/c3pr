using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class UnlockCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;
        public UnlockCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            return commandContext.Command == ".unlock" && commandContext.Arguments == "";
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var riders = train.Carriages
                .SelectMany(c => c.Riders, (c, rider) => new { Rider = rider, Carriage = c })
                .Where(r => r.Rider.Name == commandContext.UserName)
                .ToList();
            if (riders.Count != 1)
            {
                if (riders.Count == 0)
                {
                    await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You don't seem to be on the train");
                }
                else
                {
                    await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You're on multiple carriages, which one did you mean?  Remember it's zero-based.");
                }
                return;
            }
            _carriageManipulationService.UnlockCarriage(riders[0].Carriage);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }
    }
}
