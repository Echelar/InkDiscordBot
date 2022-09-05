using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkDiscordBot
{
    public class VipBalance
    {
        public const string BalanceCommand = "vip-balance";
        public const string CreditCommand = "vip-credit";
        public const string DebitCommand = "vip-debit";
        public const string CheckCommand = "vip-check";

        public static Dictionary<string, int> Balances { get; } = new Dictionary<string, int>();

        public static double GetBalance(string userName)
        {
            Balances.TryGetValue(userName, out var balance);

            return balance;
        }

        public static double Credit(string userName, int amount)
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

        public static double Debit(string userName, int amount)
        {
            return Credit(userName, amount * -1);
        }
    }
}
