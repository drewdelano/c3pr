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
        readonly string _githubAppPem;
        readonly string _githubAppClientId;
        readonly string _githubAppInstallationId;
        readonly string _githubOrganizationName;
        readonly string _githubRepositoryName;
        readonly string _githubRepositoryMainBranchName;
        readonly string _githubWorkflowFile;

        public ExternalGithubActionsBuildTrigger(
            string githubAppPem,
            string githubAppClientId,
            string githubAppInstallationId,
            string githubOrganizationName,
            string githubRepositoryName,
            string githubRepositoryMainBranchName,
            string githubWorkflowFile = "dotnet-core-ci.yml")
        {
            _githubAppPem = Required(githubAppPem, nameof(githubAppPem));
            _githubAppClientId = Required(githubAppClientId, nameof(githubAppClientId));
            _githubAppInstallationId = Required(githubAppInstallationId, nameof(githubAppInstallationId));
            _githubOrganizationName = Required(githubOrganizationName, nameof(githubOrganizationName));
            _githubRepositoryName = Required(githubRepositoryName, nameof(githubRepositoryName));
            _githubRepositoryMainBranchName = Required(githubRepositoryMainBranchName, nameof(githubRepositoryMainBranchName));
            _githubWorkflowFile = Required(githubWorkflowFile, nameof(githubWorkflowFile));
        }

        class AccessTokenHolder
        {
            public string Token { get; set; }
        }

        public async Task TriggerBuild(SlackMessageStorage storage)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAccessTokenFromPem());
            client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("C3PR"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            var tokenUrl = $"https://api.github.com/app/installations/{_githubAppInstallationId}/access_tokens";
            using var tokenResponse = await client.PostAsync(tokenUrl, null);
            tokenResponse.EnsureSuccessStatusCode();
            var accessToken = JsonConvert.DeserializeObject<AccessTokenHolder>(
                await tokenResponse.Content.ReadAsStringAsync());
            if (string.IsNullOrWhiteSpace(accessToken?.Token))
            {
                throw new InvalidOperationException("GitHub returned an empty installation access token.");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            var postBody = JsonConvert.SerializeObject(new { @ref = _githubRepositoryMainBranchName });
            var dispatchUrl = $"https://api.github.com/repos/{_githubOrganizationName}/{_githubRepositoryName}/actions/workflows/{_githubWorkflowFile}/dispatches";
            using var result = await client.PostAsync(
                dispatchUrl,
                new StringContent(postBody, Encoding.UTF8, "application/json"));

            if (!result.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"GitHub workflow dispatch failed with HTTP {(int)result.StatusCode} ({result.ReasonPhrase}).");
            }
        }

        string GenerateAccessTokenFromPem()
        {
            var header = EncodeObject(new { alg = "RS256", typ = "JWT" });
            var payload = EncodeObject(new
            {
                iat = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddMinutes(4).ToUnixTimeSeconds(),
                iss = _githubAppClientId
            });

            using var rsa = RSA.Create();
            rsa.ImportFromPem(_githubAppPem.ToCharArray());
            var signature = EncodeBytes(rsa.SignData(
                Encoding.UTF8.GetBytes($"{header}.{payload}"),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1));
            return $"{header}.{payload}.{signature}";
        }

        static string EncodeObject(object value) =>
            EncodeBytes(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));

        static string EncodeBytes(byte[] bits) =>
            Convert.ToBase64String(bits).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        static string Required(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A value is required.", name);
            }
            return value;
        }
    }
}
