using System;
using System.Text;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using C3PR.Core;
using C3PR.Core.Services;
using Microsoft.Extensions.Configuration;
using SlackNet;
using SlackNet.Bot;

namespace C3PR.Api.EntryPoint
{
    public class C3prAwsLambdaContainer : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(cc =>
            {
                var configuration = cc.Resolve<IConfiguration>();
                return new SlackApiClient(Required(configuration, "BotOauthToken"));
            }).As<ISlackApiClient>();

            ScanAndRegister(builder, "Service");
            ScanAndRegister(builder, "Command");
            builder.RegisterModule<C3prCoreModule>();

            builder.Register(cc =>
            {
                var configuration = cc.Resolve<IConfiguration>();
                var githubAppPem = Encoding.UTF8.GetString(
                    Convert.FromBase64String(Required(configuration, "GithubAppPrivateKeyBase64")));

                return new ExternalGithubActionsBuildTrigger(
                    githubAppPem,
                    Required(configuration, "GithubAppClientId"),
                    Required(configuration, "GithubAppInstallationId"),
                    Required(configuration, "GithubOrgName"),
                    Required(configuration, "GithubRepoName"),
                    Required(configuration, "GithubRepoMainBranchName"),
                    configuration.GetValue<string>("GithubWorkflowFile") ?? "dotnet-core-ci.yml");
            }).AsImplementedInterfaces();
        }

        static string Required(IConfiguration configuration, string name)
        {
            var value = configuration.GetValue<string>(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Required configuration '{name}' is missing.");
            }
            return value;
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
