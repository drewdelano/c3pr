using System.Threading.Tasks;
using C3PR.Core.Commands;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using Moq;
using NUnit.Framework;

namespace C3PR.Tests
{
    public class SlackApiDaemonServiceTests
    {
        [Test]
        public async Task SetShipUrlPostsTheDeploymentLinkWithoutSlackbotStorage()
        {
            const string shipUrl = "https://github.com/example/project/actions/runs/123";
            var slack = new Mock<ISlackApiService>(MockBehavior.Strict);
            slack.Setup(service => service.GetChannelTopic("ship-it")).ReturnsAsync("");
            slack.Setup(service => service.FormatAtHere()).ReturnsAsync("<!here>");
            slack.Setup(service => service.PostMessage(
                    "ship-it",
                    It.Is<string>(message => message.Contains(shipUrl))))
                .Returns(Task.CompletedTask);

            await new SlackApiDaemonService(new ICommand[0], slack.Object)
                .SetShipUrl("ship-it", shipUrl);

            slack.Verify(service => service.ReadLatestMessageToSelf(), Times.Never);
            slack.VerifyAll();
        }
    }
}
