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

        private const string Locale = "en-US";
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
            var executingUser = command.User.Username;
            var userOption = command.GetUser()?.Username ?? string.Empty;
            var amountOption = command.GetAmount();

            switch (command.Data.Name)
            {
                case VipBalance.BalanceCommand:
                    await command.RespondAsync($"Your balance is {VipBalance.GetBalance(executingUser)}", ephemeral: true);
                    break;
                case VipBalance.DebitCommand:
                    await command.RespondAsync($"Debited {amountOption:0,000} from {userOption} - balance is {VipBalance.Debit(userOption, amountOption):0,000}", ephemeral: true);
                    break;
                case VipBalance.CreditCommand:
                    await command.RespondAsync($"Credited {amountOption:0,000} to {userOption} - balance is {VipBalance.Credit(userOption, amountOption):0,000}", ephemeral: true);
                    break;
                case VipBalance.CheckCommand:
                    await command.RespondAsync($"{userOption}'s balance is {VipBalance.GetBalance(userOption):0,000}", ephemeral: true);
                    break;
            }
            
        }

        private async Task Client_Ready()
        {
            var guildId = _client.Guilds.First().Id;

            var guild = _client.GetGuild(guildId);

            var balanceCommand = new SlashCommandBuilder()
                .WithName(VipBalance.BalanceCommand)
                .AddNameLocalization(Locale, VipBalance.BalanceCommand)       
                .WithDescription("Check my VIP balance!")
                .AddDescriptionLocalization(Locale, "Check my VIP balance!");

            var checkCommand = new SlashCommandBuilder()
                .WithName(VipBalance.CheckCommand)
                .AddNameLocalization(Locale, VipBalance.CheckCommand)
                .WithDescription("Check another user's VIP balance")
                .AddDescriptionLocalization(Locale, "Check another user's VIP balance")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithNameLocalizations(new Dictionary<string, string> { { Locale, "user" } })
                    .WithType(ApplicationCommandOptionType.User)
                    .WithDescription("The user to check")
                    .AddDescriptionLocalization(Locale, "The user to check")
                    .WithRequired(true));

            var creditCommand = new SlashCommandBuilder()
                .WithName(VipBalance.CreditCommand)
                .AddNameLocalization(Locale, VipBalance.CreditCommand)
                .WithDescription("Add VIP credit to a user")
                .AddDescriptionLocalization(Locale, "Add VIP credit to a user")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithNameLocalizations(new Dictionary<string, string> { { Locale, "user" } })
                    .WithType(ApplicationCommandOptionType.User)
                    .WithDescription("The user to credit")
                    .AddDescriptionLocalization(Locale, "The user to credit")
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("amount")
                    .WithNameLocalizations(new Dictionary<string, string> { { Locale, "amount" } })
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithDescription("The amount of credit to add")
                    .AddDescriptionLocalization(Locale, "The amount of credit to add")
                    .WithRequired(true));

            var debitCommand = new SlashCommandBuilder()
                .WithName(VipBalance.DebitCommand)
                .AddNameLocalization(Locale, VipBalance.DebitCommand)
                .WithDescription("Remove VIP credit from a user")
                .AddDescriptionLocalization(Locale, "Remove VIP credit from a user")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithNameLocalizations(new Dictionary<string, string> { { Locale, "user" } })
                    .WithType(ApplicationCommandOptionType.User)
                    .WithDescription("The user to debit")
                    .AddDescriptionLocalization(Locale, "The user to debit")
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("amount")
                    .WithNameLocalizations(new Dictionary<string, string> { { Locale, "amount" } })
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithDescription("The amount of credit to remove")
                    .AddDescriptionLocalization(Locale, "The amount of credit to remove")
                    .WithRequired(true));

            try
            {
                await guild.CreateApplicationCommandAsync(balanceCommand.Build());
                await guild.CreateApplicationCommandAsync(checkCommand.Build());
                await guild.CreateApplicationCommandAsync(creditCommand.Build());
                await guild.CreateApplicationCommandAsync(debitCommand.Build());
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