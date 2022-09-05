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
        public static int GetAmount(this SocketSlashCommand command)
        {
            return Convert.ToInt32(command.Data.Options.FirstOrDefault(o => o.Name == "amount")?.Value ?? 0);
        }

        public static SocketGuildUser? GetUser(this SocketSlashCommand command)
        {
            return command.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value as SocketGuildUser;
        }
    }
}
