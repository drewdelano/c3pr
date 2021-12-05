using C3PR.Core.Framework.Slack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace C3PR.Core.Services
{
    public class ExternalBuildTrigger : IExternalBuildTrigger
    {
        public async Task TriggerBuild(SlackMessageStorage storage)
        {
            throw new NotImplementedException();
        }
    }
}
