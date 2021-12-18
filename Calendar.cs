using DSharpPlus.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace HLeBot
{
    public static class Calendar
    {
        static string ApplicationName;
        static CalendarService Service;
        static string CalendarId;

        static Calendar()
        {
            string googleAddress = Environment.GetEnvironmentVariable("GOOGLE_ADDRESS");
            string googleSecret = Environment.GetEnvironmentVariable("GOOGLE_SECRET").Replace("\\n", "");
            var xCred = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(googleAddress)
            {
                Scopes = new[] {
                    CalendarService.Scope.Calendar,
                    CalendarService.Scope.CalendarEvents
                }
            }.FromPrivateKey(googleSecret));

            // Create Google Calendar API service.
            Service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = xCred,
                ApplicationName = ApplicationName,
            });

            CalendarId = Environment.GetEnvironmentVariable("GOOGLE_CALENDAR");
            ApplicationName = "Google Calendar API .NET Quickstart";
        }

        public static Event GetNextEvent()
        {
            var events = GetNextEvents();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                return events.Items.First();
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
                return null;
            }
        }

        public static Events GetNextEvents(int maxResult = 1)
        {
            // Define parameters of request.
            EventsResource.ListRequest request = Service.Events.List(CalendarId);
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = maxResult;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            return request.Execute();
        }

        public static void CreateEvent(string name, DateTime startDate, DateTime endDate, string description, string location)
        {
            Service.Events.Insert(new Event() { Summary = name, Start = new EventDateTime() { DateTime = startDate }, End = new EventDateTime() { DateTime = endDate }, Description = description, Location = location }, CalendarId).Execute();
        }

        public static void DeleteEvent(string eventId)
        {
            Service.Events.Delete(CalendarId, eventId).Execute();
        }

        public static DiscordEmbed CreateEmbed(Event eventItem)
        {
            var locationString = eventItem.Location.Split(" - ")[1];
            return (new DiscordEmbedBuilder()
            {
                Title = eventItem.Summary,
                Url = "https://www.google.com/maps/search/?api=1&query=" + Uri.EscapeUriString(locationString),
                Color = DiscordColor.DarkBlue,
                Description = DateTime.Parse(eventItem.Start.DateTime.ToString()).ToString("dddd dd MMMM yyyy, H:mm", CultureInfo.CreateSpecificCulture("fr-FR")) + "\nChez : " + eventItem.Location + "\n" + eventItem.Description
            }).Build();
        }
    }
}
