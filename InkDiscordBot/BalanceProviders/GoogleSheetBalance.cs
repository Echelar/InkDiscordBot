using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace InkDiscordBot
{
    public class GoogleSheetBalanceProvider : IBalanceProvider
    {
        /// <summary>
        /// The unique ID of the spreadsheet (it's in the URL if you're editing it - spreadsheets/d/right_here)
        /// </summary>
        const string SpreadsheetId = "1rjs0QcDRH4uPXnWUZm7EPpNmiw2fcQBou9xLVcR96lw";

        /// <summary>
        /// SECRET - file contains all the tokens and keys to log in to the service account
        /// </summary>
        const string ServiceAccountFileName = "ink-venues-bot.json";

        // Names of the sheets/tabs - must match the live sheet!

        const string VIPSheetName = "VIPs";
        const string LifetimeVIPSheetName = "Lifetime VIP";
        const string AuditSheetName = "Bot Audit";

        // Column indexes - must match the live sheet!

        const int VIPSheetNameColumn = 0;
        const int VIPSheetAmountColumn = 7;
        const int LifetimeSheetNameColumn = 0;
        const int LifetimeSheetAmountColumn = 5;


        public async Task<double> Credit(string userName, int amount, string executingUser)
        {
            return await ReadAndUpdateSpreadsheet(userName, amount, executingUser);
        }

        public async Task<double> Debit(string userName, int amount, string executingUser)
        {
            return await ReadAndUpdateSpreadsheet(userName, amount * -1, executingUser);
        }

        public async Task<double> GetBalance(string userName)
        {
            return await ReadAndUpdateSpreadsheet(userName, null, "");
        }
    

        /// <summary>
        /// Main function to make a call to find the user in the spreadsheet and optionally update their value.
        /// Also will add an audit log entry if something was changed
        /// </summary>
        /// <param name="userName">The username in question</param>
        /// <param name="amountToAdd">The amount to credit (negative to debit). Null to make no update and return the amount only.</param>
        /// <param name="executingUser">The admin/staff member executing the action (for the audit trail)</param>
        /// <returns>The current/updated amount for the user</returns>
        private async Task<int> ReadAndUpdateSpreadsheet(string userName, int? amountToAdd, string executingUser)
        {
            var credential = GoogleCredential.FromFile(ServiceAccountFileName).CreateScoped("https://www.googleapis.com/auth/spreadsheets");

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Ink Discord Bot"
            });

            var sheetRequest = service.Spreadsheets.Get(SpreadsheetId);
            sheetRequest.IncludeGridData = true;
            var spreadsheet = await sheetRequest.ExecuteAsync();
            bool audit = false;

            int amount = -1;
            if (spreadsheet != null)
            {
                var vipSheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == VIPSheetName);
                if (vipSheet != null)
                {
                    int rowID = 0;
                    foreach (var row in vipSheet.Data.First().RowData)
                    {
                        if ((row.Values[VIPSheetNameColumn].EffectiveValue?.StringValue ?? string.Empty) == userName)
                        {
                            amount = Convert.ToInt32(row.Values[VIPSheetAmountColumn].EffectiveValue?.NumberValue ?? 0);
                            if (amountToAdd.HasValue)
                            {
                                amount += amountToAdd.Value;
                                var updateRequest = service.Spreadsheets.Values.Update(new ValueRange() { Values = amount.ValueToList() }, 
                                    SpreadsheetId, Utilities.GetRangeId(VIPSheetName, VIPSheetAmountColumn, rowID));
                                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                                await updateRequest.ExecuteAsync();
                                audit = true;
                            }
                            break;
                        }
                        rowID++;
                    }
                }
            }
            return amount;
        }
    }
}
