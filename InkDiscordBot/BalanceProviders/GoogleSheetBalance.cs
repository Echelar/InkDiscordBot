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
        /// SECRET - file contains all the tokens and keys to log in to the google service account
        /// </summary>
        const string ServiceAccountFileName = "ink-venues-bot.json";

        // Names of the sheets/tabs - must match the live sheet!

        const string VIPSheetName = "VIPs";
        const string LifetimeVIPSheetName = "Lifetime VIP";
        const string AuditSheetName = "Bot Audit";

        // Column indexes - must match the live sheet!

        const int VIPSheetNameColumn = 0;
        const int VIPSheetAmountColumn = 7;
        const int VIPSheetCourtColumn = 8;
        const int LifetimeSheetNameColumn = 0;
        const int LifetimeSheetAmountColumn = 5;
        const int LifetimeSheetCourtColumn = 6;


        public async Task<(int? Casino, int? Court)> Credit(string userName, int amount, string executingUser, bool isCasino)
        {
            return await ReadAndUpdateSpreadsheet(userName, amount, executingUser, isCasino);
        }

        public async Task<(int? Casino, int? Court)> Debit(string userName, int amount, string executingUser, bool isCasino)
        {
            return await ReadAndUpdateSpreadsheet(userName, amount * -1, executingUser, isCasino);
        }

        public async Task<(int? Casino, int? Court)> GetBalance(string userName)
        {
            return await ReadAndUpdateSpreadsheet(userName, null, "", true);
        }


        /// <summary>
        /// Main function to make a call to find the user in the spreadsheet and optionally update their value.
        /// Also will add an audit log entry if something was changed
        /// </summary>
        /// <param name="userName">The username in question</param>
        /// <param name="amountToAdd">The amount to credit (negative to debit). Null to make no update and return the amount only.</param>
        /// <param name="executingUser">The admin/staff member executing the action (for the audit trail)</param>
        /// <param name="isCasino">True if updating casino balance; false if updating court balance</param>
        /// <returns>The current/updated amount for the user</returns>
        private async Task<(int? Casino, int? Court)> ReadAndUpdateSpreadsheet(string userName, int? amountToAdd, string executingUser, bool isCasino)
        {
            int? amount = null;
            int? court = null;
            var credential = GoogleCredential.FromFile(ServiceAccountFileName).CreateScoped("https://www.googleapis.com/auth/spreadsheets");

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Ink Discord Bot"
            });

            var sheetRequest = service.Spreadsheets.Get(SpreadsheetId);
            sheetRequest.IncludeGridData = true;
            var spreadsheet = await sheetRequest.ExecuteAsync();

            if (spreadsheet != null)
            {
                bool audit = false;
                var vipSheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == VIPSheetName);
                if (vipSheet != null)
                {
                    int rowID = 0;
                    foreach (var row in vipSheet.Data.First().RowData)
                    {
                        if ((row.Values[VIPSheetNameColumn].EffectiveValue?.StringValue ?? string.Empty) == userName)
                        {
                            amount = Convert.ToInt32(row.Values[VIPSheetAmountColumn].EffectiveValue?.NumberValue ?? 0);
                            court = Convert.ToInt32(row.Values[VIPSheetCourtColumn].EffectiveValue?.NumberValue ?? 0);

                            // This will only be non-null if a command was executed that requires a balance update
                            if (amountToAdd.HasValue)
                            {
                                if (isCasino) amount += amountToAdd.Value;
                                else court += amountToAdd.Value;

                                var updateRequest = service.Spreadsheets.Values.Update(new ValueRange() { Values = isCasino ? amount.Value.ValueToList() : court.Value.ValueToList() },
                                    SpreadsheetId, Utilities.GetRangeId(VIPSheetName, isCasino ? VIPSheetAmountColumn : VIPSheetCourtColumn, rowID));
                                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                                await updateRequest.ExecuteAsync();
                                audit = true;
                            }
                            break;
                        }
                        rowID++;
                    }
                }

                // Didn't find it, search lifetime VIP
                // TODO - Make this and the above code 1 function called twice
                if (!amount.HasValue)
                {
                    var lifetimeVipSheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == LifetimeVIPSheetName);
                    if (lifetimeVipSheet != null)
                    {
                        int rowID = 0;
                        foreach (var row in lifetimeVipSheet.Data.First().RowData)
                        {
                            if ((row.Values[LifetimeSheetNameColumn].EffectiveValue?.StringValue ?? string.Empty) == userName)
                            {
                                amount = Convert.ToInt32(row.Values[LifetimeSheetAmountColumn].EffectiveValue?.NumberValue ?? 0);
                                court = Convert.ToInt32(row.Values[LifetimeSheetCourtColumn].EffectiveValue?.NumberValue ?? 0);

                                // This will only be non-null if a command was executed that requires a balance update
                                if (amountToAdd.HasValue)
                                {
                                    if (isCasino) amount += amountToAdd.Value;
                                    else court += amountToAdd.Value;

                                    var updateRequest = service.Spreadsheets.Values.Update(new ValueRange() { Values = isCasino ? amount.Value.ValueToList() : court.Value.ValueToList() },
                                        SpreadsheetId, Utilities.GetRangeId(LifetimeVIPSheetName, isCasino ? LifetimeSheetAmountColumn : LifetimeSheetCourtColumn, rowID));
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

                if (audit)
                {
                    await AddAuditTrail(service, spreadsheet, userName, executingUser, amountToAdd.GetValueOrDefault(0), isCasino ? amount.Value : court.Value, isCasino);
                }
            }
            return (amount, court);
        }

        /// <summary>
        /// Adds a line to the Bot Audit sheet noting what we changed
        /// </summary>
        /// <param name="service">The spreadsheet service</param>
        /// <param name="spreadsheet">The main spreadsheet</param>
        /// <param name="userName">The user whose balance was changed</param>
        /// <param name="executingUser">The staff/admin who executed the bot command</param>
        /// <param name="amountToAdd">The credit/debit amount</param>
        /// <param name="newBalance">The new balance</param>
        /// <param name="isCasino">True if casino balance, false if court balance</param>
        /// <returns></returns>
        private static async Task AddAuditTrail(SheetsService service, Spreadsheet spreadsheet, string userName, string executingUser, int amountToAdd, int newBalance, bool isCasino)
        {
            var auditSheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == AuditSheetName);
            if (auditSheet != null)
            {
                var bu = new BatchUpdateSpreadsheetRequest() { Requests = new List<Request>() };
                bu.Requests.Add(new Request()
                {
                    InsertDimension = new InsertDimensionRequest()
                    {
                        // Adds a new row at row 2
                        Range = new DimensionRange() { Dimension = "ROWS", StartIndex = 1, EndIndex = 2, SheetId = auditSheet.Properties.SheetId }
                    }
                });
                var buRequest = service.Spreadsheets.BatchUpdate(bu, SpreadsheetId);
                await buRequest.ExecuteAsync();

                IList<IList<object>> values = new List<IList<object>>();
                // These are the values to audit in the new row
                values.Add(new List<object> { DateTime.Now.ToString("g"), executingUser, userName, amountToAdd, newBalance, isCasino ? "Casino" : "Court" });

                var updateRequest = service.Spreadsheets.Values.Update(new ValueRange() { Values = values }, SpreadsheetId, $"{AuditSheetName}!A2:F2");
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                await updateRequest.ExecuteAsync();
            }
        }
    }
}
