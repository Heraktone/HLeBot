using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HLeBot
{
    public class Sheet
    {
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        static SheetsService Service = InitSheetService();
        static DriveService DService = InitDriveService();
        static string SpreadsheetId = Environment.GetEnvironmentVariable("SPREADSHEET_ID");

        static string LastChangeId = null;

        public static SheetsService InitSheetService()
        {
            // Create Google Calendar API service.
            return new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
                ApplicationName = ApplicationName,
            });
        }

        public static DriveService InitDriveService()
        {
            // Create Google Calendar API service.
            return new DriveService(new BaseClientService.Initializer()
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
                ApplicationName = ApplicationName,
            });
        }

        public static void UpdateSheet()
        {
            var today = DateTime.Now;
            var request = DService.Files.Get(SpreadsheetId);
            request.Fields = "modifiedTime";
            var modifiedTime = request.Execute().ModifiedTimeRaw;
            if (LastChangeId?.Equals(modifiedTime, StringComparison.OrdinalIgnoreCase) ?? false)
            {
                Console.WriteLine("Skipped edit because no changes found");
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
    }
}
