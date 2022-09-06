using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InkDiscordBot
{
    public class GoogleSheetBalanceProvider : IBalanceProvider
    {
        const string SpreadsheetId = "1rjs0QcDRH4uPXnWUZm7EPpNmiw2fcQBou9xLVcR96lw";


        public async Task<double> Credit(string userName, int amount)
        {
            throw new NotImplementedException();
        }

        public async Task<double> Debit(string userName, int amount)
        {
            throw new NotImplementedException();
        }

        public async Task<double> GetBalance(string userName)
        {
            return await ReadAndUpdateSpreadsheet(userName, null, "");
        }
    

        /// <summary>
        /// Main function to make a call to find the user in the spreadsheet and optionally update their value
        /// </summary>
        /// <param name="userName">The username in question</param>
        /// <param name="amountToAdd">The amount to credit (negative to debit). Null to make no update and return the amount only.</param>
        /// <param name="executingUser">The admin/staff member executing the action (for the audit trail)</param>
        /// <returns>The current/updated amount for the user</returns>
        public async Task<int> ReadAndUpdateSpreadsheet(string userName, int? amountToAdd, string executingUser)
        {
            var credential = GoogleCredential.FromFile("ink-venues-bot.json").CreateScoped("https://www.googleapis.com/auth/spreadsheets");

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Ink Discord Bot"
            });

            var sheetRequest = service.Spreadsheets.Get(SpreadsheetId);
            sheetRequest.IncludeGridData = true;
            var spreadsheet = await sheetRequest.ExecuteAsync();

            int amount = -1;
            if (spreadsheet != null)
            {
                var vipSheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == "VIPs");
                if (vipSheet == null)
                    return amount;

                foreach (var row in vipSheet.Data.First().RowData)
                {
                    if ((row.Values[0].EffectiveValue?.StringValue ?? string.Empty) == userName)
                    {
                        amount = Convert.ToInt32(row.Values[7].EffectiveValue?.NumberValue ?? 0);
                        break;
                    }
                }
            }
            return amount;
        }
    }
}
