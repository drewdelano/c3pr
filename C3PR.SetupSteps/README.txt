Setup your Bot account:
1.  Go here: https://api.slack.com/apps
2.  Click "Create New App"
3.  Name it "c3pr"
4.  Pick your workspace
5.  Click on "Install App" and install it into your workspace
6.  Make note of your "Bot User OAuth Access Token" you'll need it to install API
7.  Go to the channel you want to use the bot in Slack and "/invite @c3pr" to it

Deploy the API:
8.  Deploy the AWS Lambda to your AWS account
9.  The API expects to find the Bot User OAuth Access Token in the environmental variable "BotOauthToken" (see C3prAwsLambdaContainer).
    So copy/paste the vaue from step 6 into the AWS Console for the lambda's environmental variable named "BotOauthToken"


Finish the Bot account setup in Slack:
10. Fill out the app details from the sceenshots included
11. Re-install the app in your Slack workspace
12. Once Slack says your endpoint is verified you should be able to issue commands in the channel you invited c3pr into in step 7 
    (start with ".help" or ".join")

Prettying things up (optional):
13. Give C3PR a pretty Slack image
14. Alias some sort of meaningful emoji to the various flairs (:r: for ready, :er: for everready, :l: for lock)
15. From your build process check to see if the train is held via curl:
    curl -f https://{host}/Shipping/SafeToDeployProd?channelName={channelName}
            https://localhost:44301/Shipping/SafeToDeployProd?channelName=ship-it
    Returns:
       - 200 (OK) if the train in the channel specified is not held 
       - 418 (I'm a Tea Pot) if the train is held
       - 400 (Bad Request) is the channel name is missing
       - 409 (Conflict) if the channel name doesn't exist or C3PR hasn't been invited to it
       - 500 (Internal Server Error) if something breaks in the code

Troubleshooting:
If it isn't working, try running it locally and pointing Slack to it by using ngrok:

ngrok http -host-header=localhost 53300

From there you should be able to update your Slack "Event Subscriptions" endpoint to point to your ngrok address
(Something like "http://eb4e90457c08.ngrok.io/SlackWebhook/Event")

