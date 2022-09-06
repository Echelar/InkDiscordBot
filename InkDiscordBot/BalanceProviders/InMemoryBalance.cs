using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkDiscordBot
{
    /// <summary>
    /// In-memory implementation for testing purposes. Not for production
    /// </summary>
    public class InMemoryBalanceProvider : IBalanceProvider
    {
        private Dictionary<string, int> Balances { get; } = new Dictionary<string, int>();

        public async Task<double> GetBalance(string userName)
        {
            Balances.TryGetValue(userName, out var balance);

            return balance;
        }

        public async Task<double> Credit(string userName, int amount, string executingUser)
        {
            if (!Balances.ContainsKey(userName))
            {
                Balances.Add(userName, amount);
            }
            else
            {
                Balances[userName] += amount;
            }
            return Balances[userName];
        }

        public async Task<double> Debit(string userName, int amount, string executingUser)
        {
            return Credit(userName, amount * -1, executingUser).GetAwaiter().GetResult();
        }
    }
}
