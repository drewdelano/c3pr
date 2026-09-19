# c3pr
Communication-facilitating shipping bot

## What?
Communication is hard.  Sometimes it's nice to have a robot enforce communication changes and only have humans override the robot in exceptional circumstances.  c3pr helps navigate the relatively simple shipping cycle (who's shipping? -> merge changes -> deploy to test and test changes -> deploy to prod and prod testing) with a larger number of people than normal by handling text communication via Slack (it posts messages and uses the channel topic as storage media).

Currently, the API is written to be easily hosted inside of AWS as a Lambda, but there's no reason this shouldn't work inside of Azure. 

This is meant to be customized to your business needs by forking and changing whatever you need to.  There are extension points to include interacting with your build process and an extention point for your build to check to see if the "train" is held before shipping.  This is a lovingly developed clone of a tool written by [@robfe](https://github.com/robfe). 💕

It's tone is meant to be playful and not terrbly serious.

## Nomenclature:
__Train__ - Sort of synonymous with the channel you install c3pr into in Slack.  Common usages would be things like "get on the train" to ask a developer to join the shipping queue or "the train is held" to indicate that the train has stopped to address an issue in production.

__Carriage__ - One of the segments of the train that represents a set of developers who are going to deploy code at the same time.  Carriages are separated by "|" to indicate who is on a given carriage.

__Phase__ - Each step of the shipping cycle the developers journey through.

__Flair__ - Emojis used to represent state visually (ready, locked, held, etc.)

## How to deploy

### 1. Create the Slack app

Create a Slack app with a bot user, install it in the target workspace, and grant the bot the scopes shown in [the bot scopes screenshot](images/bot%20token%20scopes.png). Record both the Bot User OAuth Token and the app's Signing Secret. Invite `@c3pr` to the shipping channel.

Slack requests are authenticated with the signing secret. C3PR rejects requests whose signature is invalid or whose timestamp is more than five minutes old.

### 2. Create the GitHub App

Create a private GitHub App and grant only the repository `Actions: write` permission. Install it only on the repository C3PR will build. Generate a private key and record the app's client ID and installation ID. The target workflow must declare `workflow_dispatch` as a trigger.

### 3. Deploy the Lambda

Install `Amazon.Lambda.Tools`, then run this command from `src/C3PR.Api`. The deployment bucket is used by the deployment tool only; C3PR has no runtime S3 dependency.

```powershell
dotnet tool install -g Amazon.Lambda.Tools
$privateKeyBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes("c3pr.private-key.pem"))

dotnet lambda deploy-serverless `
  --configuration Release `
  --stack-name c3pr `
  --s3-bucket YOUR_DEPLOYMENT_BUCKET `
  --template serverless.template `
  --profile YOUR_AWS_PROFILE `
  --template-parameters "BotOauthToken=YOUR_BOT_TOKEN;SlackSigningSecret=YOUR_SIGNING_SECRET;C3prCallbackSecret=YOUR_CALLBACK_SECRET;GithubAppPrivateKeyBase64=$privateKeyBase64;GithubAppClientId=YOUR_CLIENT_ID;GithubAppInstallationId=YOUR_INSTALLATION_ID;GithubOrgName=YOUR_ORG;GithubRepoName=YOUR_REPO;GithubRepoMainBranchName=master;GithubWorkflowFile=dotnet-core-ci.yml"
```

The CloudFormation parameters are copied into Lambda environment variables. Parameters carrying credentials are marked `NoEcho`, and the Lambda execution role has only the basic CloudWatch Logs policy.

### 4. Finish Slack configuration

Set the Slack Event Subscriptions request URL to the stack's `SlackEventRequestURL` output, subscribe to bot `message.channels` events, reinstall the app if Slack asks, and invite the bot to the shipping channel. Start with `.help` or `.join`.

Prettying things up (optional):
1. Give C3PR a pretty Slack image
1. Alias some sort of meaningful emoji to the various flairs (:r: for ready, :er: for everready, :l: for lock, :choo: for train logo, :hold: for denoting something is wrong and needs human intervention)
1. From your build process, sign requests to the shipping API with the same `C3prCallbackSecret`. The signature base string is `v1:{unix timestamp}:{uppercase HTTP method}:{path and query}:{raw body}`. Send the timestamp in `X-C3PR-Request-Timestamp` and the lowercase HMAC-SHA256 signature as `v1={hex}` in `X-C3PR-Signature`.

The following Python example checks whether production deployment is allowed:

```python
import hashlib, hmac, os, time, urllib.parse, urllib.request

channel = "ship-it"
path = "/Shipping/SafeToDeployProd?" + urllib.parse.urlencode({"channelName": channel})
timestamp = str(int(time.time()))
canonical = f"v1:{timestamp}:GET:{path}:"
signature = "v1=" + hmac.new(
    os.environ["C3PR_CALLBACK_SECRET"].encode(), canonical.encode(), hashlib.sha256
).hexdigest()
request = urllib.request.Request(
    os.environ["C3PR_URL"].rstrip("/") + path,
    headers={"X-C3PR-Request-Timestamp": timestamp, "X-C3PR-Signature": signature},
)
urllib.request.urlopen(request)
```

Returns:
* 200 (OK) if the train in the channel specified is not held 
* 418 (I'm a Tea Pot) if the train is held
* 400 (Bad Request) if the channel name is missing
* 409 (Conflict) if the channel name doesn't exist or C3PR hasn't been invited to it
* 401 (Unauthorized) if the signature is missing, invalid, or stale
* 500 (Internal Server Error) if something breaks in the code

## Troubleshooting:
If it isn't working, try running it locally and pointing Slack to it by using ngrok:

> ngrok http -host-header=localhost 53300

From there you should be able to update your Slack "Event Subscriptions" endpoint to point to your ngrok address
(Something like "http://eb4e90457c08.ngrok.io/SlackWebhook/Event") ([exmaple](https://github.com/drewdelano/c3pr/blob/master/images/troubleshooting.png))
