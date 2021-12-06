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
    public class SlackApiDaemonService : ISlackApiDaemonService
    {
        readonly IEnumerable<ICommand> _availableCommands;
        readonly ISlackApiService _slackApiService;

        public SlackApiDaemonService(IEnumerable<ICommand> availableCommands, ISlackApiService slackApiService)
        {
            _availableCommands = availableCommands;
            _slackApiService = slackApiService;
        }

        public async Task HandleMessage(string text, string channelName, string userName)
        {
            if (!text.StartsWith("."))
            {
                return;
            }

            text = text.Replace((char)160, ' ');
            var bits = text.Split(' ', 2);
            var commandContext = new CommandContext
            {
                Command = bits[0],
                Arguments = bits.Length > 1 ? bits[1] : "",
                ChannelName = channelName,
                UserName = userName
            };

            var adapters = _availableCommands
                .Where(a => a.CanHandleMessage(commandContext))
                .ToList();
            var adapter = adapters.FirstOrDefault();
            if (adapter == null || adapters.Count > 1)
            {
                return;
            }

            await adapter.HandleMessage(commandContext);
        }

        public async Task<bool> IsChannelNameValid(string channelName)
        {
            return await _slackApiService.IsChannelNameValid(channelName);
        }

        public async Task<bool> IsSafeToShip(string channelName)
        {
            var topic = await _slackApiService.GetChannelTopic(channelName);
            var train = Train.Parse(topic);

            if (train.Flair.Contains(TrainFlair.Hold))
            {
                return false;
            }

            return true;
        }

        public async Task SetShipUrl(string channelName, string shipUrl)
        {
            var latest = await _slackApiService.ReadLatestMessageToSelf();
            latest ??= "";

            var store = SlackMessageStorage.Parse(latest);
            var channel = store.FirstOrDefault(c => c.ChannelName == channelName);
            if (channel == null)
            {
                channel = new SlackMessageStorage
                {
                    ChannelName = channelName
                };
                store.Add(channel);
            }
            channel.ShipUrl = shipUrl;
            latest = SlackMessageStorage.Stringify(store);
            await _slackApiService.PostMessage("@slackbot", latest);


            // notify in chat
            var topic = await _slackApiService.GetChannelTopic(channelName);
            var train = Train.Parse(topic);
            var driver = train.Carriages.FirstOrDefault()?.Riders.FirstOrDefault();
            if (driver != null)
            {
                var sbShippersExcludingDriver = new StringBuilder();
                foreach (var shipper in train.Carriages[0].Riders.Skip(1))
                {
                    var atShipper = await _slackApiService.FormatAtNotificationFromUserName(shipper.Name);
                    sbShippersExcludingDriver.Append($"{atShipper} ");
                }
                var shippersExcludingDriver = sbShippersExcludingDriver.ToString();
                var atDriver = await _slackApiService.FormatAtNotificationFromUserName(driver.Name);
                await _slackApiService.PostMessage(channelName, $"New build deployed! :tada:\n {shippersExcludingDriver}\n{atDriver} please co-ordinate testing.  Once everyone is .ready we can deploy to PROD.");
            }
            else
            {
                var atHere = await _slackApiService.FormatAtHere();
                await _slackApiService.PostMessage(channelName, $"New build deployed, but wasn't expected {atHere} someone needs to figure out what's going on and post an update here.");
            }

        }
    }
}
