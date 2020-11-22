using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using C3PR.Core;
using SlackNet.Bot;

namespace C3PR.TestConsole
{
    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProgramConsole>().AsImplementedInterfaces();

            builder.RegisterModule<C3prCoreModule>();
        }
    }
}
