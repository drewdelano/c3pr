using C3PR.Core.Framework.Slack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace C3PR.Core.Services
{
    public interface IExternalBuildTrigger
    {
        Task TriggerBuild(SlackMessageStorage storage);
    }
}
