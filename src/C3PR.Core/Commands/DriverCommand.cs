using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class DriverCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        readonly ICarriageManipulationService _carriageManipulationService;
        public DriverCommand(ISlackApiService slackApiService, ICarriageManipulationService carriageManipulationService)
        {
            _slackApiService = slackApiService;
            _carriageManipulationService = carriageManipulationService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".driver" && commandContext.Arguments == "")
            {
                return true;
            }

            if (commandContext.Command == ".driver" && int.TryParse(commandContext.Arguments, out var num) && num >= 0)
            {
                return true;
            }

            if (commandContext.Command == ".driver" && commandContext.Arguments.Length > 0)
            {
                return true;
            }

            return false;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".driver" && commandContext.Arguments == "")
            {
                await Simple(commandContext);
            }

            if (commandContext.Command == ".driver" && int.TryParse(commandContext.Arguments, out var num) && num >= 0)
            {
                await SpecificCarriage(commandContext);
            } else if (commandContext.Command == ".driver" && commandContext.Arguments.Length > 0)
            {
                await SpecificNewDriver(commandContext);
            }
        }

        async Task SpecificCarriage(CommandContext commandContext)
        {
            var topic = await _slackApiService.GetChannelTopic(commandContext.ChannelName);
            var train = Train.Parse(topic);

            var specificCarriageNumber = int.Parse(commandContext.Arguments);
            if (specificCarriageNumber >= train.Carriages.Count)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: Only {train.Carriages.Count} carriages are available and are zero-based.");
                return;
            }

            var carriage = train.Carriages[specificCarriageNumber];
            var rider = carriage.Riders.FirstOrDefault(r => r.Name == commandContext.UserName);
            if (rider == null)
            {
                await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: You aren't on that carriage.");
                return;
            }

            rider.Flairs.Flairs.Remove(RiderFlair.Ready);
            rider.Flairs.Flairs.Remove(RiderFlair.EverReady);
            rider.Flairs.PostflairWhitepace = "";
            rider.TrailingWhitespace = " ";
            carriage.Riders.Remove(rider);
            carriage.Riders.Insert(0, rider);
            carriage.Riders.Last().TrailingWhitespace = "";

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }

        async Task SpecificNewDriver(CommandContext commandContext)
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
                    await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: {targetUser} doesn't seem to be on the train");
                }
                else
                {
                    await _slackApiService.PostMessage(commandContext.ChannelName, $"{commandContext.UserName}: {targetUser} is on multiple carriages, which one did you mean?  Remember it's zero-based.");
                }
                return;
            }
            var rider = riders.First();
            rider.Rider.Flairs.Flairs.Remove(RiderFlair.Ready);
            rider.Rider.Flairs.Flairs.Remove(RiderFlair.EverReady);
            rider.Rider.Flairs.PostflairWhitepace = "";
            rider.Rider.TrailingWhitespace = " ";
            rider.Carriage.Riders.Remove(rider.Rider);
            rider.Carriage.Riders.Insert(0, rider.Rider);
            rider.Carriage.Riders.Last().TrailingWhitespace = "";

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
            rider.Rider.Flairs.Flairs.Remove(RiderFlair.Ready);
            rider.Rider.Flairs.Flairs.Remove(RiderFlair.EverReady);
            rider.Rider.Flairs.PostflairWhitepace = "";
            rider.Rider.TrailingWhitespace = " ";
            rider.Carriage.Riders.Remove(rider.Rider);
            rider.Carriage.Riders.Insert(0, rider.Rider);
            rider.Carriage.Riders.Last().TrailingWhitespace = "";

            await _slackApiService.SetChannelTopic(commandContext.ChannelName, train.ToString());
        }
    }
}
