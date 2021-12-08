using DSharpPlus.Entities;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HLeBot
{
    public class Calendar
    {
        static string ApplicationName = "Google Calendar API .NET Quickstart";

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
            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                ApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY"),
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List(Environment.GetEnvironmentVariable("GOOGLE_CALENDAR"));
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = maxResult;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            return request.Execute();
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
