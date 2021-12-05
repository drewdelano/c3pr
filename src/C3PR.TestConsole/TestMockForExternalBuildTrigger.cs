using C3PR.Core.Framework.Slack;
using C3PR.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace C3PR.TestConsole
{
    public class TestMockForExternalBuildTrigger : IExternalBuildTrigger
    {
        readonly TestMockForSlack _testMock;

        public TestMockForExternalBuildTrigger(TestMockForSlack testMock)
        {
            _testMock = testMock;
        }

        public async Task TriggerBuild(SlackMessageStorage storage)
        {
            var buildInfos = SlackMessageStorage.Parse(_testMock.SlackBotsLatestMessage);

            var buildInfo = buildInfos.Single(bi => bi.ChannelName == "#ship-it");
            buildInfo.ShipUrl = "https://www.microsoft.com";

            _testMock.SlackBotsLatestMessage = SlackMessageStorage.Stringify(buildInfos);
        }
    }
}