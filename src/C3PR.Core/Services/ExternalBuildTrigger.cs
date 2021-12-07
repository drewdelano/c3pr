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
        string _githubPersonalAccessToken;
        string _githubRepositoryBuildDispatch;

        public ExternalBuildTrigger(string githubPersonalAccessToken, string githubRepositoryBuildDispatch)
        {
            _githubPersonalAccessToken = githubPersonalAccessToken;
            _githubRepositoryBuildDispatch = githubRepositoryBuildDispatch;
        }

        public async Task TriggerBuild(SlackMessageStorage storage)
        {
            // Example of how to configure this for Github Actions repository_dispatch
            // https://pakstech.com/blog/github-actions-repository-dispatch/
            // Replace all PLACEHOLDER_ s in this file with the appropriate data

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _githubPersonalAccessToken);
            client.DefaultRequestHeaders.Host = "api.github.com";
            client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("C3PR"));

            var postBody = "{\"event_type\": \"c3pr_build\"}";
            var result = await client.PostAsync(_githubRepositoryBuildDispatch, new StringContent(postBody));

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Report that things didn't work as planned");
            }
        }
    }
}
