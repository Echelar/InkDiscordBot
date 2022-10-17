namespace InkDiscordBot
{
    /// <summary>
    /// Interface so we can swap implementations easily
    /// </summary>
    public interface IBalanceProvider
    {
        Task<(int? Casino, int? Court)> GetBalance(string userName);
        Task<(int? Casino, int? Court)> Credit(string userName, int amount, string executingUser, bool isCasino);
        Task<(int? Casino, int? Court)> Debit(string userName, int amount, string executingUser, bool isCasino);
    }
}
