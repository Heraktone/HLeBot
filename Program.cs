using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HLeBot
{
    class Program
    {
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        public IServiceProvider _service { get; private set; }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // How much logging do you want to see?
                LogLevel = LogSeverity.Info,

                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                //MessageCacheSize = 50,

                // If your platform doesn't have native WebSockets,
                // add Discord.Net.Providers.WS4Net from NuGet,
                // add the `using` at the top, and uncomment this line:
                //WebSocketProvider = WS4NetProvider.Instance
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                // Again, log level:
                LogLevel = LogSeverity.Info,

                // There's a few more properties you can set,
                // for example, case-insensitive commands.
                CaseSensitiveCommands = false,
            });

            //And add theme to IServiceProvider. Sevice provider can help you to bind your objects.
            _service = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .BuildServiceProvider();

            _client.MessageReceived += Client_MessageReceived;

            // Subscribe the logging handler to both the client and the CommandService.
            _client.Log += Log;
            _commands.Log += Log;
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _service);

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };

            Calendar.GetNextEvents();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }


        //Fired when we receive message
        private async Task Client_MessageReceived(SocketMessage arg)
        {
            int argPos = 0;
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            if (message.HasStringPrefix("!", ref argPos))//If message has string prefix we execute command…
            {
                var result = await _commands.ExecuteAsync(context, argPos, _service);//Executing…

                if (!result.IsSuccess)//If something went wrong…
                {
                    await context.Channel.SendMessageAsync(embed: new EmbedBuilder
                    {
                        Description = "Error",
                        Color = Color.Red
                    }.Build());
                    Console.WriteLine("Error");
                }
                else
                    Console.WriteLine("Succesfull");
                //Nice
            }
        }

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}

    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ckan")]
        [Summary("Dis moi c'est quand la prochaine séance.")]
        public Task CKanAsync()
        {
            return ReplyAsync(Calendar.GetNextEvents());
        }

        [Command("help")]
        [Summary("Donne la liste des commandes.")]
        public Task HelpAsync()
            => ReplyAsync("`!ckan` vous donne la prochaine séance.");

    }


    public class Calendar
    {
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        public static string GetNextEvents()
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
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                var eventItem = events.Items.First();
                return "Voila : \n" + eventItem.Start.DateTime.ToString() + "\n" + eventItem.Summary + "\n" + eventItem.Location + "\n" + eventItem.Description;
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
                return "Je n'ai aucune date";
            }
        }
    }
}
