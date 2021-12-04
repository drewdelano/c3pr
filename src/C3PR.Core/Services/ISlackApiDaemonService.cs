using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace C3PR.Core.Services
{
    public interface ISlackApiDaemonService
    {
        Task HandleMessage(string text, string channelName, string userName);
        Task<bool> IsSafeToShip(string channelName);
        Task<bool> IsChannelNameValid(string channelName);
        Task SetShipUrl(string channelName, string shipUrl);
    }
}
