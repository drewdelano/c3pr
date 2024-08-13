using C3PR.Core.Framework.Slack;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace C3PR.Core.Services
{
    public class ExternalGithubActionsBuildTrigger : IExternalBuildTrigger
    {
        string _githubAppPem;
        string _githubAppClientId;
        string _githubAppInstallationId;
        string _githubOrganizationName;
        string _githubRepositoryName;
        string _githubRepositoryMainBranchName;

        public ExternalGithubActionsBuildTrigger(
            string githubAppPem,
            string githubAppClientId,
            string githubAppInstallationId,
            string githubOrganizationName,
            string githubRepositoryName,
            string githubRepositoryMainBranchName)
        {
            _githubAppPem = githubAppPem;
            _githubAppClientId = githubAppClientId;
            _githubAppInstallationId = githubAppInstallationId;
            _githubOrganizationName = githubOrganizationName;
            _githubRepositoryName = githubRepositoryName;
            _githubRepositoryMainBranchName = githubRepositoryMainBranchName;
        }

        class AccessTokenHolder
        {
            public string Token { get; set; }
        }

        public async Task TriggerBuild(SlackMessageStorage storage)
        {
            // Example of how to configure this for Github Actions repository_dispatch
            // https://pakstech.com/blog/github-actions-repository-dispatch/
            // Replace all PLACEHOLDER_ s in this file with the appropriate data

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAccessTokenFromPem());
            client.DefaultRequestHeaders.Host = "api.github.com";
            client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("C3PR"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var installationId = _githubAppInstallationId;
            var githubGetTokenUrl = $"https://api.github.com/app/installations/{installationId}/access_tokens";
            var accessTokenHolderRaw = await client.PostAsync(githubGetTokenUrl, null);
            var accessTokenHolder = JsonConvert.DeserializeObject<AccessTokenHolder>(await accessTokenHolderRaw.Content.ReadAsStringAsync());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenHolder.Token);

            var postBody = JsonConvert.SerializeObject(new
            {
                @ref = _githubRepositoryMainBranchName
            });
            var org = _githubOrganizationName;
            var repo = _githubRepositoryName;
            var githubRepositoryBuildDispatchUrl = $"https://api.github.com/repos/{org}/{repo}/actions/workflows/dotnet-core-ci.yml/dispatches";
            var result = await client.PostAsync(githubRepositoryBuildDispatchUrl, new StringContent(postBody));

            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Report that things didn't work as planned");
            }
        }

        private string GenerateAccessTokenFromPem()
        {
            // from:
            // https://docs.github.com/en/apps/creating-github-apps/authenticating-with-a-github-app/generating-a-json-web-token-jwt-for-a-github-app#example-using-powershell-to-generate-a-jwt

            var header = EncodeObject(new
            {
                alg = "RS256",
                typ = "JWT"
            });

            var payload = EncodeObject(new
            {
                iat = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddMinutes(4).ToUnixTimeSeconds(),
                iss = _githubAppClientId
            });

            var rsa = RSA.Create();
            rsa.ImportFromPem(_githubAppPem.ToCharArray());

            var signature = EncodeBytes(
                rsa.SignData(UTF8.GetBytes($"{header}.{payload}"),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1));

            return $"{header}.{payload}.{signature}";
        }

        private string EncodeObject(object value)
        {
            var json = JsonConvert.SerializeObject(value);
            var bits = UTF8.GetBytes(json);
            return EncodeBytes(bits);
        }

        private string EncodeBytes(byte[] bits)
        {
            return Convert.ToBase64String(bits)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private Encoding UTF8 => Encoding.UTF8;
    }
}
