using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3PR.Core.Commands;
using C3PR.Core.Framework;
using C3PR.Core.Framework.Slack;

namespace C3PR.Core.Services
{
    public class CarriageManipulationService : ICarriageManipulationService
    {
        ISlackApiService _slackApiService;
        IExternalBuildTrigger _externalBuildTriggerService;
        public CarriageManipulationService(ISlackApiService slackApiService, IExternalBuildTrigger externalBuildTriggerService)
        {
            _slackApiService = slackApiService;
            _externalBuildTriggerService = externalBuildTriggerService;
        }

        public void AddRiderInNewCarriage(Train train, string userName)
        {
            if (!train.IsEmpty)
            {
                train.Carriages.Last().Riders.Last().TrailingWhitespace += " ";
            }
            else
            {
                if (train.Flair.Count == 0)
                {
                    train.Flair.Add(TrainFlair.Train);
                }
                train.Phase = Phase.Rollcall;
            }
            train.Carriages.Add(new Carriage
            {
                Riders = new List<Rider>
                {
                    new Rider
                    {
                        Flairs = new RiderFlairContainer
                        {
                            PreflairWhitepace = " ",
                            Flairs = new List<RiderFlair>(),
                            PostflairWhitepace = ""
                        },
                        Name = userName
                    }
                }
            });
        }

        public async Task AdvanceCarriageIfNecessary(Train train, string channelName)
        {
            var isReadyToAdvance = true;
            foreach (var rider in train.Carriages[0].Riders)
            {
                var isReady = rider.Flairs.Flairs.Contains(RiderFlair.Ready);
                var isEverReady = rider.Flairs.Flairs.Contains(RiderFlair.EverReady);
                if (!(isReady || isEverReady))
                {
                    isReadyToAdvance = false;
                    break;
                }
            }
            await _slackApiService.SetChannelTopic(channelName, train.ToString());

            if (isReadyToAdvance)
            {
                await AdvanceCarriage(train, channelName);
            }
        }

        async Task AdvanceCarriage(Train train, string channelName)
        {
            // clear ready
            foreach (var rider in train.Carriages[0].Riders)
            {
                if (rider.Flairs.Flairs.Contains(RiderFlair.Ready))
                {
                    rider.Flairs.Flairs.Remove(RiderFlair.Ready);
                    rider.Flairs.PostflairWhitepace = "";
                }
            }
            train.Carriages[0].Riders[0].Flairs.Flairs.Remove(RiderFlair.EverReady);

            await HandlePhaseAdvancement(train, channelName);
            if (train.Phase == Phase.Testing)
            {
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
                    // trigger the build
                    await _externalBuildTriggerService.TriggerBuild(channel);
                    await _slackApiService.PostMessage(channelName, $"Starting the build...");
                }
                catch (Exception ex)
                {
                    var atHere = await _slackApiService.FormatAtHere();
                    await _slackApiService.PostMessage(channelName, $"Something went wrong triggering the build... {atHere}");
                }
            }

            string deploymentUrl = null;
            if (train.Phase == Phase.Production)
            {
                var messageToSelf = await _slackApiService.ReadLatestMessageToSelf();
                var channels = SlackMessageStorage.Parse(messageToSelf);

                var channel = channels.FirstOrDefault(c => c.ChannelName == channelName);
                if (channel?.ShipUrl != null)
                {
                    // link to deploy url in channel
                    deploymentUrl = channel.ShipUrl;
                }
                else
                {
                    deploymentUrl = "Isn't configured for some reason...";
                }
            }

            if (train.Carriages.Count > 0)
            {
                await HandleBeginPhaseMessaging(train, channelName, deploymentUrl);
            }
        }

        async Task HandleBeginPhaseMessaging(Train train, string channelName, string deploymentUrl)
        {
            var shippers = $"{string.Join("\n", train.Carriages[0].Riders.Select(r => r.Name))}\n\n";
            var driver = train.Carriages[0].Riders[0].Name;
            if (train.Phase == Phase.Rollcall)
            {
                await _slackApiService.PostMessage(channelName, $"{shippers}Everybody ready-up and let's get this train a-rollin'");
            }
            else if (train.Phase == Phase.Merging)
            {
                await _slackApiService.PostMessage(channelName, $"{shippers}Merge your PRs when you're ready and then .ready to indicate that you're done");
            }
            else if (train.Phase == Phase.Testing)
            {
                await _slackApiService.PostMessage(channelName, $"{shippers}Wait for the build to finish deploying and then work with QA to get things tested");
            }
            else if (train.Phase == Phase.Production)
            {
                await _slackApiService.PostMessage(channelName, $"{shippers}{driver} deploy the build, make sure prod testing happens if needed, also watch the metrics and rayguns to make sure everything is OK");
                await _slackApiService.PostMessage(channelName, $"Deploy URL: {deploymentUrl}");
            }
            else
            {
                await _slackApiService.PostMessage(channelName, $"I'm confused about what phase we're in");
            }
        }

        async Task<bool> HandlePhaseAdvancement(Train train, string channelName)
        {
            if (train.Phase == Phase.Rollcall)
            {
                train.Phase = Phase.Merging;
            }
            else if (train.Phase == Phase.Merging)
            {
                train.Phase = Phase.Testing;
            }
            else if (train.Phase == Phase.Testing)
            {
                train.Phase = Phase.Production;
            }
            else if (train.Phase == Phase.Production)
            {
                train.Phase = Phase.Rollcall;
                train.Carriages.RemoveAt(0);
            }
            else
            {
                await _slackApiService.PostMessage($"I'm not quite sure what phase we're in.  {train.Carriages[0].Riders[0].Name} can you take over?  I'm feeling faint.", channelName);
                return false;
            }

            return true;
        }

        public void LockCarriage(Carriage carriage)
        {
            carriage.Flairs.PreflairWhitespace = " ";
            carriage.Flairs.Flairs.Add(CarriageFlair.Lock);
        }

        public void UnlockCarriage(Carriage carriage)
        {
            carriage.Flairs.PreflairWhitespace = "";
            carriage.Flairs.Flairs.Remove(CarriageFlair.Lock);
        }

        public void AddRiderToCarriageByIndex(Train train, string userName, int specificCarriageNumber)
        {
            var lastRider = train.Carriages[specificCarriageNumber].Riders.Last();
            lastRider.TrailingWhitespace = " ";

            var newRider = new Rider
            {
                Flairs = new RiderFlairContainer
                {
                    PreflairWhitepace = " "
                },
                Name = userName
            };
            train.Carriages[specificCarriageNumber].Riders.Add(newRider);
            if (specificCarriageNumber < train.Carriages.Count - 1)
            {
                newRider.TrailingWhitespace = " ";
            }
        }

        public void RemoveRiderFromCarriage(Train train, Rider rider, Carriage carriage)
        {
            if (carriage.Riders.Count == 1)
            {
                train.Carriages.Remove(carriage);
                return;
            }

            carriage.Riders.Remove(rider);

            var idx = train.Carriages.IndexOf(carriage);
            if (idx == train.Carriages.Count - 1)
            {
                train.Carriages[idx].Riders.Last().TrailingWhitespace = "";
            }
            else
            {
                train.Carriages[idx].Riders.Last().TrailingWhitespace = " ";
            }
        }

        public bool IsPhaseUnknown(Phase phase)
        {
            return Phase.KnownPhases.Contains(phase) == false;
        }

        public bool IsNotHeld(Train train, Rider currentRider)
        {
            var isReadyToAdvance = true;
            foreach (var rider in train.Carriages[0].Riders)
            {
                if (rider.Name == currentRider.Name)
                {
                    continue;
                }

                var isReady = rider.Flairs.Flairs.Contains(RiderFlair.Ready);
                var isEverReady = rider.Flairs.Flairs.Contains(RiderFlair.EverReady);
                if (!(isReady || isEverReady))
                {
                    isReadyToAdvance = false;
                    break;
                }
            }

            if (!isReadyToAdvance)
            {
                return false;
            }

            if (train.Phase == Phase.Testing)
            {
                return false;
            }

            return train.Flair.Contains(TrainFlair.Hold);
        }
    }
}
