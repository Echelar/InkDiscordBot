namespace InkDiscordBot
{
    /// <summary>
    /// Interface so we can swap implementations easily
    /// </summary>
    public interface IBalanceProvider
    {
        Task<double> GetBalance(string userName);
        Task<double> Credit(string userName, int amount);
        Task<double> Debit(string userName, int amount);
    }
}
