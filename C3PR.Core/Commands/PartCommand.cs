using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class PartCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;

        public PartCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".part" && commandContext.Arguments == "")
            {
                return true;
            }

            if (commandContext.Command == ".part" && int.TryParse(commandContext.Arguments, out var num) && num >= 0)
            {
                return true;
            }

            return false;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".part" && commandContext.Arguments == "")
            {
                await Simple(commandContext);
            }

            if (commandContext.Command == ".part" && int.TryParse(commandContext.Arguments, out var num) && num >= 0)
            {
                await Specific(commandContext);
            }
        }

        async Task Specific(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var specificCarriageNumber = int.Parse(commandContext.Arguments);
            if (specificCarriageNumber >= train.Carriages.Count)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: The carriage you asked for doesn't exist (it's zero-based, remember?).");
                return;
            }

            var rider = train.Carriages[specificCarriageNumber].Riders.FirstOrDefault(r => r.Name == commandContext.UserName);
            if (rider == null)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You aren't on that carriage");
                return;
            }

            if (specificCarriageNumber == 0 && train.Phase != Phase.Rollcall)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You can't get off the train after its left the station.");
                return;
            }

            _carriageManipulationService.RemoveRiderFromCarriage(train, rider, train.Carriages[specificCarriageNumber]);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }

        async Task Simple(CommandContext commandContext)
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
            var rider = riders.First();

            if (train.Carriages.IndexOf(rider.Carriage) == 0 && train.Phase != Phase.Rollcall)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You can't get off the train after its left the station.");
                return;
            }

            _carriageManipulationService.RemoveRiderFromCarriage(train, rider.Rider, rider.Carriage);

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }
    }
}
