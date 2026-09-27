using System.Threading.Tasks;
using C3PR.Core.Commands;
using C3PR.Core.Framework;
using C3PR.Core.Framework.Slack;
using C3PR.Core.Services;
using Moq;
using NUnit.Framework;

namespace C3PR.Tests
{
    public class BuildCommandTests
    {
        [Test]
        public async Task BuildDispatchesWithoutReadingLegacySlackbotStorage()
        {
            var slack = new Mock<ISlackApiService>(MockBehavior.Strict);
            slack.Setup(service => service.GetChannelTopic("#ship-it"))
                .ReturnsAsync(":choo: <prod> @driver");
            slack.Setup(service => service.PostMessage("#ship-it", "Running the build pipeline..."))
                .Returns(Task.CompletedTask);
            var trigger = new Mock<IExternalBuildTrigger>(MockBehavior.Strict);
            trigger.Setup(service => service.TriggerBuild(It.Is<SlackMessageStorage>(storage =>
                    storage.ChannelName == "#ship-it" && storage.ShipUrl == "")))
                .Returns(Task.CompletedTask);

            await new BuildCommand(slack.Object, trigger.Object).HandleMessage(new CommandContext
            {
                Command = ".build",
                ChannelName = "#ship-it",
                UserName = "@driver"
            });

            slack.Verify(service => service.ReadLatestMessageToSelf(), Times.Never);
            trigger.VerifyAll();
        }
    }
}
