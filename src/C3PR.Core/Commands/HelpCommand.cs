using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using C3PR.Core.Framework;
using C3PR.Core.Services;
using SlackNet.Blocks;

namespace C3PR.Core.Commands
{
    public class HelpCommand : ICommand
    {
        readonly ISlackApiService _slackApiService;
        public HelpCommand(ISlackApiService slackApiService)
        {
            _slackApiService = slackApiService;
        }

        public bool CanHandleMessage(CommandContext commandContext)
        {
            if (commandContext.Command == ".help" && commandContext.Arguments == "")
            {
                return true;
            }

            return false;
        }

        public async Task HandleMessage(CommandContext commandContext)
        {
            var helpText = $@"
I'm C3PR!  I'll help you co-ordinate shipping frequent releases!

The idea is that a group of developers (a carriage) will self-organize to deploy pieces of code behind feature flags.
Once they're ready to go we'll move through each of the phases defined below until the release is in production.
The first person on the train is known as the driver and they're responsible for knowing what's shipping, what's at risk, and verifying stats at the end of the release.
Carriages can be locked to indicate high risk and communicate that no further shippers may join.

Order of phases:
rollcall -> merging -> qa -> prod

rollcall = Organizing the who and what and making sure everyone is ready to go.  This is the last chance to say something is ""too risky"" to ship in it's current state.  If a PR is risky, bump it to a later carriage and lock it so it can be deployed in isolation.
merging = Everyone is agreed.  Let's merge our PRs and get this show on the road.
qa = As we enter this stage the build will automatically trigger, we wait for it to finish and then test our PRs with our QAs to make sure everything works and doesn't break anything else.
prod = Devs / QAs are satisified with the results.  The driver begins the process of deploying the release into PROD, afterwards Dev / QA test to make sure PROD is still functional and all important stats look good.

Commands:
.add @person 0 = Adds @person to the first carriage on the train (zero-based).
.driver = Make yourself the driver.
.everready = Mark yourself as ready for all stages (good if you're only shipping test changes or something else small and safe).
.help = What you're reading right now.
.join = Add a new carriage to the train with only you in it.
.kick @person 0 = Removes @person from the first carriage.
.lock 0 = Locks the first carriage.
.part 0 = Leaves the first carriage (you can only do this before merging).
.ready = Communicate that you're ready for the next step in the process.
.ready @person = Communicates that you've talked with @person and they are ready to go.
.unlock 0 = Unlocks first carriage.
.unready = Unmarks yourself as ready.
.build = Trigger the creation of a new build.
";
            await _slackApiService.PostMessage(commandContext.ChannelName, helpText);
        }
    }
}
