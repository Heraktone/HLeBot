﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Quartz;
using Quartz.Logging;
using System;
using System.Threading.Tasks;

namespace HLeBot
{
    public class Program
    {
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

        private static DiscordClient Client;
        private static readonly ulong PlayerIdStr = 500148339883376641;
        private static readonly ulong ScenarioChannelId = 623907090297258005;
        private static readonly ulong LoloChannelId = 882470660776026152;
        public IServiceProvider Service { get; private set; }

        public async static Task<DiscordChannel> GetScenarioChannel()
        {
            return await Client.GetChannelAsync(ScenarioChannelId);
        }

        public async static Task<DiscordChannel> GetLoloChannel()
        {
            return await Client.GetChannelAsync(LoloChannelId);
        }

        public static string GetPlayerIdStr()
        {
            return $"<@&{PlayerIdStr}>";
        }

        public async Task MainAsync()
        {
            LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());

            // Discord
            Client = new DiscordClient(new DiscordConfiguration
            {
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN"),
                TokenType = TokenType.Bot
            });

            var commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" }
            });
            commands.RegisterCommands<Commands>();
            commands.SetHelpFormatter<CustomHelpFormatter>();

            IScheduler sched = await SchedulerBuilder.Create()
                .UseDefaultThreadPool(x => x.MaxConcurrency = 1)
                .BuildScheduler();

            await sched.Start();

            var jobsAndTrigger = QuartzJobs.CreateJobsAndTrigger();

            await Client.ConnectAsync();
            Console.WriteLine("Bot is connected!");
            foreach(var jobAndTrigger in jobsAndTrigger)
            {
                await sched.ScheduleJob(jobAndTrigger.Job, jobAndTrigger.Trigger);
            }

            // Block this task until the program is closed.
            await Task.Delay(-1);

            await sched.Shutdown();
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
