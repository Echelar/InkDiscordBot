using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Configuration;

namespace InkDiscordBot
{
    public class Program 
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        private DiscordSocketClient _client;
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += Client_SlashCommandExecuted;
            var token = ConfigurationManager.AppSettings["token"];
            
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
            
        }

        private async Task Client_SlashCommandExecuted(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case VipBalance.BalanceCommand:
                    await command.RespondAsync($"Your balance is {VipBalance.GetBalance(command.User.Username)}", ephemeral: true);
                    break;
                case VipBalance.DebitCommand:
                    await command.RespondAsync($"Debited {0} - balance is {0})";
                    break;
            }
            
        }

        private async Task Client_Ready()
        {
            var guildId = _client.Guilds.First().Id;

            // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
            var guild = _client.GetGuild(guildId);

            // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
            var guildCommand = new SlashCommandBuilder();

            // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            guildCommand.WithName(VipBalance.BalanceCommand);
            guildCommand.AddNameLocalization("en-US", VipBalance.BalanceCommand);

            // Descriptions can have a max length of 100.
            guildCommand.WithDescription("Check my VIP balance!");
            guildCommand.AddDescriptionLocalization("en-US", "Check my VIP balance!");

            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                await guild.CreateApplicationCommandAsync(guildCommand.Build());

                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch (HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                await Log(new LogMessage(LogSeverity.Error, "Client_ready", json, exception));
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}