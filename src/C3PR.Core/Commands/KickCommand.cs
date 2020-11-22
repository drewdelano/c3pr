using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;
using SlackNet.WebApi;

namespace C3PR.Core.Commands
{
    public class KickCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;
        public KickCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            return commandContext.Command == ".kick" && commandContext.Arguments.Length > 1 && commandContext.Arguments.Split(' ').Length == 1;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var targetUser = await _slackApiService.GetUserFromId(commandContext.Arguments);
            var riders = train.Carriages
                .SelectMany(c => c.Riders, (c, rider) => new { Rider = rider, Carriage = c })
                .Where(r => r.Rider.Name == targetUser)
                .ToList();
            if (riders.Count != 1)
            {
                if (riders.Count == 0)
                {
                    await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Can't kick {targetUser}, they don't seem to be on the train");
                }
                else
                {
                    await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Can't kick {targetUser}, they're on multiple carriages, which one did you mean?  Remember it's zero-based.");
                }
                return;
            }
            var rider = riders.First();


            _carriageManipulationService.RemoveRiderFromCarriage(train, rider.Rider, rider.Carriage);
            
            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }
    }
}
