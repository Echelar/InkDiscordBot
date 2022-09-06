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

        public const string BalanceCommand = "vip-balance";
        public const string CreditCommand = "vip-credit";
        public const string DebitCommand = "vip-debit";
        public const string CheckCommand = "vip-check";
        public const string Locale = "en-US";
        private DiscordSocketClient _client;

        // Swappable implementation if needed
        private IBalanceProvider _balanceProvider = new GoogleSheetBalanceProvider();

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.None
            };
            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += Client_SlashCommandExecuted;

            ///
            /// Keep this secret - treat this as the bot's login and password all in one
            /// Can be reset at https://discord.com/developers/applications/1016147035390488577/bot "reset token" (or sub your app id)
            ///
            var token = ConfigurationManager.AppSettings["token"];
            
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
            
        }

        /// <summary>
        /// This handles a command when a user executes one
        /// </summary>
        private async Task Client_SlashCommandExecuted(SocketSlashCommand command)
        {
            var executingUser = command.User.Username;
            var userOption = command.GetUser()?.Username ?? string.Empty;
            var amountOption = command.GetAmount();

            // Any operation that takes longer than 3 seconds MUST be deferred and then edited later
            await command.DeferAsync(true);
            double balance = 0;
            switch (command.Data.Name)
            {
                case BalanceCommand:
                    balance = await _balanceProvider.GetBalance(executingUser);
                    await command.ModifyOriginalResponseAsync(mp => mp.Content = $"Your balance is {balance:#,##0}");
                    break;
                case DebitCommand:
                    balance = await _balanceProvider.Debit(userOption, amountOption, executingUser);
                    await command.ModifyOriginalResponseAsync(mp => mp.Content = $"Debited {amountOption:#,##0} from {userOption} - balance is {balance:#,##0}");
                    break;
                case CreditCommand:
                    balance = await _balanceProvider.Credit(userOption, amountOption, executingUser);
                    await command.ModifyOriginalResponseAsync(mp => mp.Content = $"Credited {amountOption:#,##0} to {userOption} - balance is {balance:#,##0}");
                    break;
                case CheckCommand:
                    balance = await _balanceProvider.GetBalance(userOption);
                    await command.ModifyOriginalResponseAsync(mp => mp.Content = $"{userOption}'s balance is {balance:#,##0}");
                    break;
            }
        }

        /// <summary>
        /// This sets up all the app commands when the bot connects
        /// </summary>
        private async Task Client_Ready()
        {
            var guildId = _client.Guilds.First().Id;

            var guild = _client.GetGuild(guildId);

            var balanceCommand = new SlashCommandBuilder()
                .WithName(BalanceCommand)
                .AddNameLocalization(Locale, BalanceCommand) // Note: The API doesn't have good null protection so you HAVE to set up localization apparently
                .WithDescription("Check my VIP balance!")
                .AddDescriptionLocalization(Locale, "Check my VIP balance!");

            var checkCommand = new SlashCommandBuilder()
                .WithName(CheckCommand)
                .AddNameLocalization(Locale, CheckCommand)
                .WithDescription("Check another user's VIP balance")
                .AddDescriptionLocalization(Locale, "Check another user's VIP balance")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("user")
                    .WithNameLocalizations(new Dictionary<string, string> { { Locale, "user" } }) // Note: due to a bug in the api you have to do this instead of .AddNameLocalization
                    .WithType(ApplicationCommandOptionType.User)
                    .WithDescription("The user to check")
                    .AddDescriptionLocalization(Locale, "The user to check")
                    .WithRequired(true));

            var creditCommand = new SlashCommandBuilder()
                .WithName(CreditCommand)
                .AddNameLocalization(Locale, CreditCommand)
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
                .WithName(DebitCommand)
                .AddNameLocalization(Locale, DebitCommand)
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