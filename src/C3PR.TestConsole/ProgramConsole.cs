using System;
using System.Threading.Tasks;
using C3PR.Core.Commands;
using C3PR.Core.Framework;
using C3PR.Core.Services;

namespace C3PR.TestConsole
{
    public class ProgramConsole : IProgramConsole
    {
        readonly ISlackApiService _slackApiService;
        readonly ISlackApiDaemonService _slackApiDaemonService;

        public ProgramConsole(ISlackApiService slackApiService, ISlackApiDaemonService slackApiDaemonService)
        {
            _slackApiService = slackApiService;
            _slackApiDaemonService = slackApiDaemonService;
        }

        public async Task Run()
        {
            try
            {
                await Main();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task Main()
        {
            Console.WriteLine("C3PR test bed to see if everything is working how you think it should be.");
            Console.WriteLine("Use ~ and then a user name to switch users");
            Console.WriteLine();
            Console.Write("Initial Topic: ");
            var topic = Console.ReadLine();
            await _slackApiService.SetChannelTopic("#ship-it", topic);

            var currentUser = "@wendy.darling";
            while (true)
            {
                Console.Write($"{currentUser}: ");
                var line = Console.ReadLine();
                if (line.StartsWith("~"))
                {
                    currentUser = line.Substring(1);
                }
                else
                {
                    await _slackApiDaemonService.HandleMessage(line, "#ship-it", currentUser);
                }
            }
        }
    }
}
