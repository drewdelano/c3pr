using System;
using System.Threading.Tasks;
using Autofac;
using Moq;

namespace C3PR.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<TestModule>();
            containerBuilder.RegisterType<TestMockForSlack>()
                .AsImplementedInterfaces()
                .SingleInstance();

            var container = containerBuilder.Build();

            await container.Resolve<IProgramConsole>().Run();
        }
    }
}
