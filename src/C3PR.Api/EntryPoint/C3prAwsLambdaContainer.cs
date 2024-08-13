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
                var botOauthToken = configuration.GetValue<string>("BotOauthToken");
                var slackClient = new SlackApiClient(botOauthToken);

                return slackClient;
            }).As<ISlackApiClient>();

            ScanAndRegister(builder, "Service");
            ScanAndRegister(builder, "Command");

            builder.RegisterModule<C3prCoreModule>();


            builder.Register(cc =>
            {
                var configuration = cc.Resolve<IConfiguration>();
                var githubAppPem = configuration.GetValue<string>("githubAppPem");
                var githubAppClientId = configuration.GetValue<string>("githubAppClientId");
                var githubAppInstallationId = configuration.GetValue<string>("githubAppInstallationId");
                var githubOrgName = configuration.GetValue<string>("githubOrgName");
                var githubRepoName = configuration.GetValue<string>("githubRepoName");
                var githubRepoMainBranchName = configuration.GetValue<string>("githubRepoMainBranchName");

                var slackClient = new ExternalGithubActionsBuildTrigger(
                    githubAppPem,
                    githubAppClientId,
                    githubAppInstallationId,
                    githubOrgName,
                    githubRepoName,
                    githubRepoMainBranchName);

                return slackClient;
            }).AsImplementedInterfaces();
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