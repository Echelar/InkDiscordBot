# Intro

This is a discord bot I created to be a quick interface between users (and admins) and a google document that stores some credits for a VIP system. It uses the [Discord.Net](https://github.com/discord-net/Discord.Net) package to interface with Discord, and the [Google Sheets .NET Client](https://github.com/googleapis/google-api-dotnet-client) package to interface with google sheets. It is written in C# and targets .NET 6.0.

# Steps to get it running for the first time

There are a number of "secrets" (passwords, tokens, etc) that I have not included in this repository since they would grant access to various accounts or resources I own, or might dox me in some way. Therefore, to get this up and running on your machine, take the following steps:

- Clone the code/repo to your machine (I use [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/community/) to code, but obviously there are other choices)
- Go to the [Discord App Dev Portal](https://discord.com/developers/applications) and create a new Application. Under the Bot tab, there will be a Token that you can see only once (after that it is permanently hidden and you'll need to generate a new one if you lose it). Copy the token and paste it into the code in the file 'app.config' in the value=" " where it says Replace Me. This will allow the app to log in and take control of this bot.
- Create yourself a new discord server that you own so you can invite the bot and test it out
- In the [Discord App Dev Portal](https://discord.com/developers/applications) for your application, go to the OAuth2 section. This will let you create a unique URL that invites your bot to your server. Check 'bot' under the list of scopes, which will open up a new list of bot permissions. Check 'Send Messages'. There should now be a URL at the bottom that you can copy into a new browser window. It will bring up a discord prompt asking you to authorize the bot to join a server - pick the test server you created (you have to be the owner to do this). Now the bot should show up as an offline member of your server in discord.
- Get a copy of the ink VIP spreadsheet for your own use (Requirements: sheet "VIPs" has name in column A, $ credit in column H, court credit in column I; sheet "Lifetime VIP" has name in column A, $ credit in column F, court credit in column G). If any of these need to be changed in the code, these names/numbers can be found in GoogleSheetBalance.cs. 
  - Create a 4th sheet: "Bot Audit" with columns (in order): "Date", "Staff Member", "VIP User", "Credit/Debit", "New Balance", "Credit/Debit Type" (names do not have to be exact). This will be where the bot will record history.
  - Update the sheet ID in the code - this string will be in the URL when you're editing the sheet, and the ID goes in GoogleSheetBalance.cs in the const string SpreadsheetId
- Create a google service account that will be the credentials we use to 'log in' and change data.
  - Go to https://console.cloud.google.com/ and log in with the account you wish to control these credentials (can use a personal acct - I don't think this is visible anywhere in the spreadsheet or otherwise). Create a new Project, give it any name you like. 
  - Once you have created and are in your new project, search for 'service account' - you want one that says something like "IAM & Admin/Service Accounts". Once at that tab, click Create Service Account, fill out some reasonable values for the name and id and description.
  - Go to the Keys tab of the service account and click 'Add Key'. Accept the 'json' format and confirm this. It will download a .json file to your machine -- move this to your code folder and put it in the root directory (alongside 'Program.cs') and name it 'ink-venues-bot.json' (hardcoded filename in GoogleSheetBalance.cs). This file contains all information necessary to authenticate as this service account and should be kept secret.
  - Back on the google cloud console site: you should now see the 'email' that represents your service account - you can now go to your google spreadsheet and add this account email as an Editor of the sheet
- You should now be able to run the bot by starting the application in visual studio (or whatever IDE you use). After about 10 seconds, when the console reads 'Ready', the bot should come online in Discord. You can now see the commands by typing / in discord to start the slash commands.

# Hosting

Permanent hosting is something that we will want to figure out. Currently, I have it hosted in microsoft azure as a Web Job underneath a blank App Service. Because it needs to be online 24/7 and not idled out, I had to pick a higher pay tier than free, so it costs about $30/m. I get some free credits as a part of my irl job, so it doesn't actually cost me anything, but I think there are likely many better options to host it.

Check some of these out and look for pros/cons I suppose:

https://geekflare.com/discord-bot-hosting/

https://www.writebots.com/discord-bot-hosting/

