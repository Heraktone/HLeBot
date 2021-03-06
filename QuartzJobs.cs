using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HLeBot
{
    public class JobsAndTrigger
	{
		public IJobDetail Job;
		public ITrigger Trigger;
	}

	public static class QuartzJobs
    {
		public static IEnumerable<JobsAndTrigger> CreateJobsAndTrigger()
        {
			var jobsAndTrigger = new List<JobsAndTrigger>();

			jobsAndTrigger.Add(new JobsAndTrigger() { 
				Job = JobBuilder.Create<NotifyChannelJob>()
					.WithIdentity("job1", "group1")
					.Build(), 
				Trigger = TriggerBuilder.Create()
					.WithIdentity("trigger1", "group1")
					.StartNow()
					.WithCronSchedule("0 0 16 * * ?")
					.Build()
			});

			jobsAndTrigger.Add(new JobsAndTrigger()
			{
				Job = JobBuilder.Create<NotifyHungry>()
					.WithIdentity("job2", "group2")
					.Build(),
				Trigger = TriggerBuilder.Create()
					.WithIdentity("trigger2", "group2")
					.StartNow()
					.WithCronSchedule("0 0 * * * ?")
					.Build()
			});

			jobsAndTrigger.Add(new JobsAndTrigger()
			{
				Job = JobBuilder.Create<UpdateCalendar>()
					.WithIdentity("job3", "group3")
					.Build(),
				Trigger = TriggerBuilder.Create()
					.WithIdentity("trigger3", "group3")
					.StartNow()
					.WithCronSchedule("0 * * * * ?")
					.Build()
			});
			return jobsAndTrigger;
        }
    }

	public class NotifyHungry : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var nextEvent = Calendar.GetNextEvent();
			if (nextEvent == null || !nextEvent.Start.DateTime.HasValue || (int)(DateTime.Today - nextEvent.Start.DateTime.Value.Date).TotalDays != 0 || nextEvent.Location.ToLowerInvariant().Contains("discord"))
			{
				return;
			}

			var totalHoursDiff = (DateTime.Now.TimeOfDay.Hours - nextEvent.Start.DateTime.Value.TimeOfDay.Hours);
			if (totalHoursDiff == -3)
			{
				var embed = Utils.CreateEmbedGF1();
				var chnl = await Program.GetScenarioChannel();
				var message = await chnl.SendMessageAsync($"{Program.GetPlayerIdStr()} On mange quoi ?", embed.Reponse);
				await Utils.SendReactionsToMessage(message, embed.Emotes.ToList());
				await message.CreateThreadAsync($"Discussion sur le repas du {DateTime.Now.Date.ToString("dd-MM-yyyy")}", DSharpPlus.AutoArchiveDuration.Hour);
			}
		}
	}

	public class NotifyChannelJob : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			var nextEvent = Calendar.GetNextEvent();
			if (nextEvent == null || !nextEvent.Start.DateTime.HasValue)
			{
				return;
			}

			var totalDaysDiff = (int)(DateTime.Today - nextEvent.Start.DateTime.Value.Date).TotalDays;
			if (totalDaysDiff == -1)
            {
				var embed = Calendar.CreateEmbed(nextEvent);
				var chnl = await Program.GetScenarioChannel();
				await chnl.SendMessageAsync($"{Program.GetPlayerIdStr()}{(nextEvent.Location.Contains("Thibault") ? "<@&134346294608003072>" : "")} N'oubliez pas !", embed);
				await Console.Out.WriteLineAsync("Reminder sent");
			}
		}
	}

	public class UpdateCalendar : IJob
	{
		public async Task Execute(IJobExecutionContext context)
		{
			await Task.Factory.StartNew(() => { Sheet.UpdateSheet(); });
		}
	}
}
