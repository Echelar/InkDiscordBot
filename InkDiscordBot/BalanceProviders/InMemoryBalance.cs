namespace InkDiscordBot
{
    /// <summary>
    /// In-memory implementation for testing purposes. Not for production
    /// </summary>
    public class InMemoryBalanceProvider : IBalanceProvider
    {
        private Dictionary<string, int> Balances { get; } = new Dictionary<string, int>();

        public async Task<(int? Casino, int? Court)> GetBalance(string userName)
        {
            Balances.TryGetValue(userName, out var balance);

            return (balance, null);
        }

        public async Task<(int? Casino, int? Court)> Credit(string userName, int amount, string executingUser, bool isCasino)
        {
            if (!Balances.ContainsKey(userName))
            {
                Balances.Add(userName, amount);
            }
            else
            {
                Balances[userName] += amount;
            }
            return (Balances[userName], null);
        }

        public async Task<(int? Casino, int? Court)> Debit(string userName, int amount, string executingUser, bool isCasino)
        {
            return Credit(userName, amount * -1, executingUser, isCasino).GetAwaiter().GetResult();
        }
    }
}
