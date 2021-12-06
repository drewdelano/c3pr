using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using C3PR.Core;
using C3PR.Core.Commands;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using Moq;
using NUnit.Framework;

namespace C3PR.Tests
{
    public class TestBase
    {
        static Mock<ISlackApiService> _mockSlackApiService = new Mock<ISlackApiService>();
        static IContainer _container;

        static TestBase()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<C3prCoreModule>();
            containerBuilder.RegisterInstance(new Mock<IExternalBuildTrigger>().Object).AsImplementedInterfaces();
            containerBuilder.RegisterInstance(_mockSlackApiService.Object).AsImplementedInterfaces();
            _container = containerBuilder.Build();
        }

        public async Task CommandLineTest<T>(string currentUser, string channelTopicBeforeTest, string message, string channelTopicAfterTest, string replyMessage = null) where T : ICommand
        {
            // Arrange
            string newTopic = null;
            string errorMessage = null;
            _mockSlackApiService.Setup(m => m.GetChannelTopic("#ship-it"))
                .ReturnsAsync(channelTopicBeforeTest);
            _mockSlackApiService.Setup(m => m.SetChannelTopic("#ship-it", It.IsAny<string>()))
                .Callback<string, string>((_, topic) => newTopic = topic);
            _mockSlackApiService.Setup(m => m.GetUserFromId(It.IsAny<string>()))
                .ReturnsAsync<string, ISlackApiService, string>(s => s);
            _mockSlackApiService.Setup(m => m.PostMessage("#ship-it", It.IsAny<string>()))
                .Callback<string, string>((_, message) => errorMessage = message);
            _mockSlackApiService.Setup(m => m.FormatAtNotificationFromUserName(It.IsAny<string>()))
                .Returns<string>(userId => Task.FromResult(userId));

            var ctx = new CommandContext
            {
                Command = message.Split(' ', 2).First(),
                Arguments = message.Split(' ', 2).Skip(1).LastOrDefault() ?? "",
                ChannelName = "#ship-it",
                UserName = currentUser,
            };
            var commands = _container.Resolve<IEnumerable<ICommand>>();
            var commandToExecute = commands.Single(c => c.CanHandleMessage(ctx));
            Assert.AreEqual(typeof(T).FullName, commandToExecute.GetType().FullName);

            // Act
            await commandToExecute.HandleMessage(ctx);

            // Assert
            Assert.AreEqual(channelTopicAfterTest, newTopic);
            Assert.AreEqual(replyMessage, errorMessage);
        }
    }
}
