using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkDiscordBot
{
    public static class Utilities
    {
        /// <summary>
        /// Pulls out the 'amount' option from the command (and converts to int)
        /// </summary>
        public static int GetAmount(this SocketSlashCommand command)
        {
            return Convert.ToInt32(command.Data.Options.FirstOrDefault(o => o.Name == "amount")?.Value ?? 0);
        }

        /// <summary>
        /// Pulls out the 'user' option from the command
        /// </summary>
        public static SocketGuildUser? GetUser(this SocketSlashCommand command)
        {
            return command.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value as SocketGuildUser;
        }

        /// <summary>
        /// Pulls the 'credit-type' option from the command
        /// </summary>
        public static string? GetCreditDebitType(this SocketSlashCommand command)
        {
            return command.Data.Options.FirstOrDefault(o => o.Name == "credit-type")?.Value as string;
        }

        /// <summary>
        /// For setting a "range" of one cell to one value (i.e. amount) it requires a complex type
        /// </summary>
        /// <param name="amount">The $$$ amount</param>
        /// <returns>List of list of object, like the api wants</returns>
        public static IList<IList<object>> ValueToList(this int amount)
        {
            IList<IList<object>> list = new List<IList<object>>();
            list.Add(new List<object> { amount });
            return list;
        }

        /// <summary>
        /// Converts from column ID and row ID to A1 format
        /// </summary>
        /// <param name="sheetName">The sheet/tab name</param>
        /// <param name="columnID">The 0-based column id</param>
        /// <param name="rowID">The 0-based row id</param>
        /// <returns>The range identifier, e.g. B7</returns>
        /// <remarks>This will NOT work if you go out past column Z to AA etc</remarks>
        public static string GetRangeId(string sheetName, int columnID, int rowID)
        {
            var column = (Char)(65 + columnID);
            return $"{sheetName}!{column}{rowID + 1}";
        }
    }
}
