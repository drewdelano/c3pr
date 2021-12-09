using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using C3PR.Core.Commands;
using C3PR.Core.Framework;
using C3PR.Core.Framework.Slack;
using SlackNet;
using SlackNet.Bot;
using SlackNet.WebApi;

namespace C3PR.TestConsole
{
    public class TestMockForSlack : ISlackApiClient, IConversationsApi, IUsersApi, IChatApi
    {
        string _topic;

        public IChatApi Chat => this;

        public IConversationsApi Conversations => this;

        public async Task<string> SetTopic(string channelId, string topic, CancellationToken? cancellationToken = null)
        {
            _topic = topic;
            Console.WriteLine($"Set Topic: {_topic}");

            return _topic;
        }

        public async Task<Conversation> GetConversationByName(string conversationName)
        {
            return new Conversation
            {
                Topic = new Topic
                {
                    Value = _topic
                }
            };
        }

        public async Task Send(BotMessage message, CancellationToken? cancellationToken = null)
        {
            Console.WriteLine($"C3PR: {message.Text}");
        }


        public async Task<ConversationListResponse> List(bool excludeArchived = false, int limit = 100, IEnumerable<ConversationType> types = null, string cursor = null, CancellationToken? cancellationToken = null)
        {
            return new ConversationListResponse
            {
                Channels = new List<Conversation>
                {
                    new Conversation
                    {
                        Id = "#ship-it",
                        Name = "#ship-it",
                        Topic = new Topic
                        {
                            Value = _topic
                        }
                    }
                }
            };
        }

        async Task<UserListResponse> IUsersApi.List(string cursor = null, bool includeLocale = false, int limit = 0, CancellationToken? cancellationToken = null)
        {
            return new UserListResponse
            {
                Members = new List<User>
                {
                    new User
                    {
                        Name = "wendy.darling",
                        Id = "U123wendy.darling"
                    },
                    new User
                    {
                        Name = "captain.hook",
                        Id = "U123captain.hook"
                    },
                    new User
                    {
                        Name = "peter.pan",
                        Id = "U123peter.pan"
                    },
                    new User
                    {
                        Name = "tinkerbell",
                        Id = "U123tinkerbell"
                    },
                    new User
                    {
                        Name = "john.darling",
                        Id = "U123john.darling"
                    },
                    new User
                    {
                        Name = "slackbot",
                        Id = "USLACKBOT"
                    },
                }
            };
        }


        async Task<User> IUsersApi.Info(string userId, bool includeLocale = false, CancellationToken? cancellationToken = null)
        {
            return new User
            {
                Name = userId,
                Id = userId
            };
        }


        async Task<PostMessageResponse> IChatApi.PostMessage(Message message, CancellationToken? cancellationToken)
        {
            Console.WriteLine($"C3PR: {message.Text}");
            return new PostMessageResponse
            {
            };
        }


        async Task<ConversationOpenResponse> IConversationsApi.OpenAndReturnInfo(IEnumerable<string> userIds, CancellationToken? cancellationToken = null)
        {
            return new ConversationOpenResponse
            {
                Channel = new Conversation
                {
                    Id = userIds.Single()
                }
            };
        }

        public string SlackBotsLatestMessage { get; internal set; } = SlackMessageStorage.Stringify(new List<SlackMessageStorage>
            {
                new SlackMessageStorage
                {
                    ChannelName = "#ship-it",
                    ShipUrl = ""
                }
            });
        async Task<ConversationHistoryResponse> IConversationsApi.History(string channelId, string latestTs = null, string oldestTs = null, bool inclusive = false, int limit = 100, string cursor = null, CancellationToken? cancellationToken = null)
        {
            return new ConversationHistoryResponse
            {
                Latest = SlackBotsLatestMessage
            };
        }

        #region unused stuff

        public string Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public IObservable<IMessage> Messages => throw new NotImplementedException();

        public IApiApi Api => throw new NotImplementedException();

        public IAuthApi Auth => throw new NotImplementedException();

        public IBotsApi Bots => throw new NotImplementedException();

        //public IChannelsApi Channels => throw new NotImplementedException();


        public IDialogApi Dialog => throw new NotImplementedException();

        public IDndApi Dnd => throw new NotImplementedException();

        public IEmojiApi Emoji => throw new NotImplementedException();

        public IFileCommentsApi FileComments => throw new NotImplementedException();

        public IFilesApi Files => throw new NotImplementedException();

        public IMigrationApi Migration => throw new NotImplementedException();

        public IOAuthApi OAuth => throw new NotImplementedException();

        public IPinsApi Pins => throw new NotImplementedException();

        public IReactionsApi Reactions => throw new NotImplementedException();

        public IRemindersApi Reminders => throw new NotImplementedException();

        public IRemoteFilesApi RemoteFiles => throw new NotImplementedException();

        public IRtmApi Rtm => throw new NotImplementedException();

        public IScheduledMessagesApi ScheduledMessages => throw new NotImplementedException();

        public ISearchApi Search => throw new NotImplementedException();

        public IStarsApi Stars => throw new NotImplementedException();

        public ITeamApi Team => throw new NotImplementedException();

        public ITeamProfileApi TeamProfile => throw new NotImplementedException();

        public IUserGroupsApi UserGroups => throw new NotImplementedException();

        public IUserGroupUsersApi UserGroupUsers => throw new NotImplementedException();

        public IUsersApi Users => this;

        public IUserProfileApi UserProfile => throw new NotImplementedException();

        public IViewsApi Views => throw new NotImplementedException();

        public IAppsConnectionsApi AppsConnectionsApi { get; }
        public IAppsEventsAuthorizationsApi AppsEventsAuthorizations { get; }
        public IOAuthV2Api OAuthV2 { get; }
        public IOpenIdApi OpenIdApi { get; }
        public IWorkflowsApi Workflows { get; }

        public event EventHandler<IMessage> OnMessage;

        public void AddHandler(IMessageHandler handler)
        {
            throw new NotImplementedException();
        }

        public void AddIncomingMiddleware(Func<IObservable<IMessage>, IObservable<IMessage>> middleware)
        {
            throw new NotImplementedException();
        }

        public void AddOutgoingMiddleware(Func<IObservable<BotMessage>, IObservable<BotMessage>> middleware)
        {
            throw new NotImplementedException();
        }

        public Task Archive(string channelId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public void ClearCache()
        {
            throw new NotImplementedException();
        }

        public Task Close(string channelId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task Connect(CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Conversation> Create(string name, bool isPrivate, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task Get(string apiMethod, Dictionary<string, object> args, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> Get<T>(string apiMethod, Dictionary<string, object> args, CancellationToken? cancellationToken) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<BotInfo> GetBotUserById(string botId)
        {
            throw new NotImplementedException();
        }

        public Task<Hub> GetChannelByName(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Channel>> GetChannels()
        {
            throw new NotImplementedException();
        }

        public Task<Conversation> GetConversationById(string conversationId)
        {
            throw new NotImplementedException();
        }

        public Task<Conversation> GetConversationByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<Conversation>> GetConversations()
        {
            throw new NotImplementedException();
        }

        public Task<Hub> GetGroupByName(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Channel>> GetGroups()
        {
            throw new NotImplementedException();
        }

        public Task<Hub> GetHubById(string hubId)
        {
            throw new NotImplementedException();
        }

        public Task<Hub> GetHubByName(string channel)
        {
            throw new NotImplementedException();
        }

        public Task<Im> GetImByName(string username)
        {
            throw new NotImplementedException();
        }

        public Task<Im> GetImByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Im>> GetIms()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Channel>> GetMpIms()
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByName(string username)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<User>> GetUsers()
        {
            throw new NotImplementedException();
        }

        public Task<Conversation> Info(string channelId, bool includeLocale = false, bool includeNumMembers = false, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<Conversation> Invite(string channelId, IEnumerable<string> userIds, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<ConversationJoinResponse> Join(string channelId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task Kick(string channelId, string userId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task Leave(string channelId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }


        public Task Mark(string channelId, string messageTs, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<ConversationMembersResponse> Members(string channelId, int limit = 100, string cursor = null, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(BotMessage value)
        {
            throw new NotImplementedException();
        }

        public Task<string> Open(string channelId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> Open(IEnumerable<string> userIds, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<ConversationOpenResponse> OpenAndReturnInfo(string channelId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task Post(string apiMethod, Dictionary<string, object> args, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> Post<T>(string apiMethod, Dictionary<string, object> args, CancellationToken? cancellationToken) where T : class
        {
            throw new NotImplementedException();
        }

        public Task Post(string apiMethod, Dictionary<string, object> args, HttpContent content, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> Post<T>(string apiMethod, Dictionary<string, object> args, HttpContent content, CancellationToken? cancellationToken) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<Conversation> Rename(string channelId, string name, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<ConversationMessagesResponse> Replies(string channelId, string threadTs, string latestTs = null, string oldestTs = null, bool inclusive = false, int limit = 10, string cursor = null, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task Respond(string responseUrl, IReadOnlyMessage message, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> SetPurpose(string channelId, string purpose, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task Unarchive(string channelId, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task WhileTyping(string channelId, Func<Task> action)
        {
            throw new NotImplementedException();
        }

        public ISlackApiClient WithAccessToken(string accessToken)
        {
            throw new NotImplementedException();
        }

        Task<ConversationListResponse> IUsersApi.Conversations(bool excludeArchived, int limit, IEnumerable<ConversationType> types, string userId, string cursor, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUsersApi.DeletePhoto(CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<Presence> IUsersApi.GetPresence(string userId, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<IdentityResponse> IUsersApi.Identity(CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<User> IUsersApi.LookupByEmail(string email, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUsersApi.SetPhoto(byte[] imageContent, string contentType, string fileName, int? cropW, int? cropX, int? cropY, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUsersApi.SetPhoto(Stream image, string contentType, string fileName, int? cropW, int? cropX, int? cropY, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUsersApi.SetPresence(Presence presence, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<MessageTsResponse> IChatApi.Delete(string ts, string channelId, bool asUser, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<MessageTsResponse> IChatApi.MeMessage(string channel, string text, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }


        Task<ScheduleMessageResponse> IChatApi.ScheduleMessage(Message message, DateTime postAt, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<PostMessageResponse> IChatApi.PostEphemeral(string userId, Message message, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IChatApi.Unfurl(string channelId, string ts, IDictionary<string, Attachment> unfurls, bool userAuthRequired, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<MessageUpdateResponse> IChatApi.Update(MessageUpdate messageUpdate, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<PermalinkResponse> IChatApi.GetPermalink(string channelId, string messageTs, CancellationToken? cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<User> Info(string userId, bool includeLocale = false, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<UserListResponse> List(string cursor = null, bool includeLocale = false, int limit = 0, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}