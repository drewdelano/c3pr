using System.Threading.Tasks;

namespace C3PR.Core.Commands
{
    public interface ISlackApiService
    {
        Task<string> GetChannelTopic(string channelName);

        Task SetChannelTopic(string channelName, string newTopic);

        Task PostMessage(string channelName, string message);

        Task<string> GetUserFromId(string userId);
        Task<bool> IsChannelNameValid(string channelName);
        Task<string> ReadLatestMessageToSelf();

        Task<string> FormatAtNotificationFromUserName(string userName);
        Task<string> FormatAtHere();
    }
}