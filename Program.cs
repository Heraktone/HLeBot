using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace HLeBot
{
    public class Program
    {
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

        private static DiscordSocketClient Client;
        public static CommandService Commands;
        private static readonly ulong PlayerIdStr = 500148339883376641;
        private static readonly ulong ScenarioChannelId = 623907090297258005;
        private static readonly ulong LoloChannelId = 882470660776026152;
        public IServiceProvider Service { get; private set; }

        public static IMessageChannel GetScenarioChannel()
        {
            return Client.GetChannel(ScenarioChannelId) as IMessageChannel;
        }

        public async static Task<IDMChannel> GetLoloChannel()
        {
            return await Client.GetDMChannelAsync(LoloChannelId) as IDMChannel;
        }

        public static string GetPlayerIdStr()
        {
            return $"<@&{PlayerIdStr}>";
        }

        public async Task MainAsync()
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            // Discord
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info,
                CaseSensitiveCommands = false,
            });

            Service = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton<CommandService>()
                .BuildServiceProvider();

            Client.MessageReceived += Client_MessageReceived;

            Client.Log += Log;
            Commands.Log += Log;
            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), Service);

            await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await Client.StartAsync();

            IScheduler sched = await SchedulerBuilder.Create()
                .UseDefaultThreadPool(x => x.MaxConcurrency = 1)
                .BuildScheduler();

            await sched.Start();

            var jobsAndTrigger = QuartzJobs.CreateJobsAndTrigger();

            Client.Ready += async () =>
            {
                Console.WriteLine("Bot is connected!");
                foreach(var jobAndTrigger in jobsAndTrigger)
                {
                    await sched.ScheduleJob(jobAndTrigger.Job, jobAndTrigger.Trigger);
                }
            };

            // Block this task until the program is closed.
            await Task.Delay(-1);

            await sched.Shutdown();
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            int argPos = 0;
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(Client, message);

            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await Commands.ExecuteAsync(context, argPos, Service);

                if (!result.IsSuccess)
                    Console.WriteLine("Error");
                else
                    Console.WriteLine("Succesfull");
            }
        }

        private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}

    public class ConsoleLogProvider : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (level >= Quartz.Logging.LogLevel.Info && func != null)
                {
                    Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                }
                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }
    }
}
