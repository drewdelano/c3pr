# c3pr
Communication-facilitating shipping bot

## What?
Communication is hard.  Sometimes it's nice to have a robot enforce communication changes and only have humans override the robot in exceptional circumstances.  c3pr helps navigate the relatively simple shipping cycle (who's shipping? -> merge changes -> deploy to test and test changes -> deploy to prod and prod testing) with a larger number of people than normal by handling text communication via Slack (it posts messages and uses the channel topic as storage media).

Currently, the API is written to be easily hosted inside of AWS as a Lambda, but there's no reason this shouldn't work inside of Azure. 

This is meant to be customized to your business needs by forking and changing whatever you need to.  There are extension points to include interacting with your build process and an extention point for your build to check to see if the "train" is held before shipping.  This is a lovingly developed clone of a tool written by [@mightymuke](https://github.com/mightymuke). 💕
It's tone is meant to be playful and not serious.

## Nomenclature:
__Train__ - Sort of synonymous with the channel you install c3pr into in Slack.  Common usages would be things like "get on the train" to ask a developer to join the shipping queue or "the train is held" to indicate that the train has stopped to address an issue in production.

__Carriage__ - One of the segments of the train that represents a set of developers who are going to deploy code at the same time.  Carriages are separated by "|" to indicate who is on a given carriage.

__Phase__ - Each step of the shipping cycle the developers journey through.

__Flair__ - Emojis used to represent state visually (ready, locked, held, etc.)

## How to Deploy:
Setup your Bot account:
1.  Go here: https://api.slack.com/apps
1.  Click "Create New App"
1.  Name it "c3pr"
1.  Pick your workspace
1.  Click on "Install App" and install it into your workspace
1.  Make note of your "Bot User OAuth Access Token" you'll need it to install API
1.  Go to the channel you want to use the bot in Slack and "/invite @c3pr" to it

Deploy the API:
1.  Deploy the AWS Lambda to your AWS account
1.  The API expects to find the Bot User OAuth Access Token in the environmental variable "BotOauthToken" (see C3prAwsLambdaContainer).
    So copy/paste the vaue from step 6 into the AWS Console for the lambda's environmental variable named "BotOauthToken"


Finish the Bot account setup in Slack:
1. Fill out the app details from the sceenshots included
1. Re-install the app in your Slack workspace
1. Once Slack says your endpoint is verified you should be able to issue commands in the channel you invited c3pr into in step 7 
    (start with ".help" or ".join")

Prettying things up (optional):
1. Give C3PR a pretty Slack image
1. Alias some sort of meaningful emoji to the various flairs (:r: for ready, :er: for everready, :l: for lock)
1. From your build process check to see if the train is held via curl:

    curl -f https://{host}/Shipping/SafeToDeployProd?channelName={channelName}

            https://localhost:44301/Shipping/SafeToDeployProd?channelName=ship-it

Returns:
* 200 (OK) if the train in the channel specified is not held 
* 418 (I'm a Tea Pot) if the train is held
* 400 (Bad Request) is the channel name is missing
* 409 (Conflict) if the channel name doesn't exist or C3PR hasn't been invited to it
* 500 (Internal Server Error) if something breaks in the code

## Troubleshooting:
If it isn't working, try running it locally and pointing Slack to it by using ngrok:

ngrok http -host-header=localhost 53300

From there you should be able to update your Slack "Event Subscriptions" endpoint to point to your ngrok address
(Something like "http://eb4e90457c08.ngrok.io/SlackWebhook/Event")
