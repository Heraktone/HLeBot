using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HLeBot
{
    public static class Sheet
    {
        static string ApplicationName;

        static SheetsService Service;
        static DriveService DService;
        static string SpreadsheetId;
        static string DB_Spreadsheet_Id = "1E87TQns8ZQcHX1UwCPhjDw6l41bROZZ6W7w_XikFPl0";

        static string LastChangeId = null;

        static Sheet()
        {
            // Create Google Sheets API service.
            string googleAddress = Environment.GetEnvironmentVariable("GOOGLE_ADDRESS");
            string googleSecret = Environment.GetEnvironmentVariable("GOOGLE_SECRET").Replace("\\n", "");
            var xCred = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(googleAddress)
            {
                Scopes = new[] {
                    SheetsService.Scope.Spreadsheets
                }
            }.FromPrivateKey(googleSecret));
            Service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = xCred,
                ApplicationName = ApplicationName,
            });
            DService = new DriveService(new BaseClientService.Initializer()
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
                ApplicationName = ApplicationName,
            });
            SpreadsheetId = Environment.GetEnvironmentVariable("SPREADSHEET_ID");
            ApplicationName = "Google Calendar API .NET Quickstart";
        }

        public static void UpdateSheet()
        {
            var today = DateTime.Now;
            var request = DService.Files.Get(SpreadsheetId);
            request.Fields = "modifiedTime";
            var modifiedTime = request.Execute().ModifiedTimeRaw;
            if (LastChangeId?.Equals(modifiedTime, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                // "Skipped edit because no changes found";
                return;
            }
            LastChangeId = modifiedTime;

            // Delete previous events 
            var events = Calendar.GetNextEvents(200).Items;
            foreach(var ev in events)
            {
                Console.WriteLine($"Deleting event : {ev.Summary} from {ev.Start.DateTime} to {ev.End.DateTime} at {ev.Location} with description {ev.Description}");
                Calendar.DeleteEvent(ev.Id);
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }

            // Add events from spreadsheet
            var spreadsheetContent = Service.Spreadsheets.Values.Get(SpreadsheetId, "Feuille 1!A:M").Execute();
            IList<IList<Object>> spreadsheetContentValues = spreadsheetContent.Values;

            foreach(var row in spreadsheetContentValues.Skip(1))
            {
                var startDate = DateTime.ParseExact((string)row[0], "dd/MM/yy HH:mm", null);
                if (today > startDate)
                {
                    continue;
                }
                var name = (string)row[2];
                var endDate = startDate.AddHours(4);
                var hostName = (string)row[1];
                var place = hostName + " - " + (string)spreadsheetContentValues.First(s => s.Count > 10 && ((string)s[9]).Equals(hostName, StringComparison.OrdinalIgnoreCase))[10];
                var description = "";
                if (row.Count > 3)
                {
                    description = (string)row[3];
                }

                Console.WriteLine($"Creating event : '{name}' from '{startDate}' to '{endDate}' at '{place}' with description '{description}'");
                Calendar.CreateEvent(name, startDate, endDate, description, place);
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        public static string GetUser(string user)
        {
            var spreadsheetContent = Service.Spreadsheets.Values.Get(DB_Spreadsheet_Id, "LinkNames!A2:C").Execute();
            IList<IList<Object>> spreadsheetContentValues = spreadsheetContent.Values;
            var value = spreadsheetContentValues.FirstOrDefault(r => ((string)r[2]).Equals(user, StringComparison.OrdinalIgnoreCase));
            if (value == null)
            {
                return null;
            }
            else
            {
                return (string)value[0];
            }
        }

        public static bool BuyNFT(string hash, string owner)
        {
            var spreadsheetContent = Service.Spreadsheets.Values.Get(DB_Spreadsheet_Id, "NFT!A2:B").Execute();
            IList<IList<Object>> spreadsheetContentValues = spreadsheetContent.Values;
            var value = spreadsheetContentValues?.FirstOrDefault(r => ((string)r[0]).Equals(hash, StringComparison.OrdinalIgnoreCase));
            if (value == null)
            {
                var index = spreadsheetContentValues?.Count() ?? 0;
                IList<Object> obj = new List<Object>();
                obj.Add(hash);
                obj.Add(owner);
                IList<IList<Object>> values = new List<IList<Object>>();
                values.Add(obj);

                SpreadsheetsResource.ValuesResource.AppendRequest request = Service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, DB_Spreadsheet_Id, "A:B");
                request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                var response = request.Execute();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetNFT(string hash)
        {
            var spreadsheetContent = Service.Spreadsheets.Values.Get(DB_Spreadsheet_Id, "NFT!A2:B").Execute();
            IList<IList<Object>> spreadsheetContentValues = spreadsheetContent.Values;
            var value = spreadsheetContentValues.FirstOrDefault(r => ((string)r[0]).Equals(hash, StringComparison.OrdinalIgnoreCase));
            if (value == null)
            {
                return null;
            }
            else
            {
                return (string)value[1];
            }
        }
    }
}
