using System;
using System.Collections.Generic;
using System.Text;
using Autofac;

namespace C3PR.Core
{
    public class C3prCoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            ScanAndRegister(builder, "Service");
            ScanAndRegister(builder, "Command");
        }

        void ScanAndRegister(ContainerBuilder builder, string endsWith)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                            .PublicOnly()
                            .Where(t => t.Name.EndsWith(endsWith))
                            .AsImplementedInterfaces()
                            .InstancePerLifetimeScope();
        }
    }
}
