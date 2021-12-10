using C3PR.Core.Framework.Slack;
using Newtonsoft.Json;
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
    public class ExternalGithubActionsBuildTrigger : IExternalBuildTrigger
    {
        string _githubPersonalAccessToken;
        string _githubRepositoryBuildDispatch;
        string _githubRepositoryMasterBranch;

        public ExternalGithubActionsBuildTrigger(
            string githubPersonalAccessToken,
            string githubRepositoryBuildDispatch,
            string githubRepositoryMasterBranch)
        {
            _githubPersonalAccessToken = githubPersonalAccessToken;
            _githubRepositoryBuildDispatch = githubRepositoryBuildDispatch;
            _githubRepositoryMasterBranch = githubRepositoryMasterBranch;
        }

        class BranchHeadInfo
        {
            public class BranchHeadCommitInfo
            {
                public string Message { get; set; }
            }

            public BranchHeadCommitInfo Commit { get; set; }
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

            var headResult = await client.GetAsync(_githubRepositoryMasterBranch);
            var headInfo = JsonConvert.DeserializeObject<BranchHeadInfo>(await headResult.Content.ReadAsStringAsync());

            var postBody = $"{{\"event_type\": \"{headInfo.Commit.Message}\"}}";
            var result = await client.PostAsync(_githubRepositoryBuildDispatch, new StringContent(postBody));

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Report that things didn't work as planned");
            }
        }
    }
}
