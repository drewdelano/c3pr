using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3PR.Core.Commands;
using C3PR.Core.Framework;

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
    }
}
