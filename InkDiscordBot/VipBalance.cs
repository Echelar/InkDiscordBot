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

        public static double GetBalance(string userName)
        {
            return 0;
        }

        public static double Credit(string userName, double amount)
        {
            return 0;
        }

        public static double Debit(string userName, double amount)
        {
            return 0;
        }
    }
}
