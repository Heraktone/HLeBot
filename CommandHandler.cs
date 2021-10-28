using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HLeBot
{
    public class Commands : BaseCommandModule
    {
        [Command("ckan")]
        [Description("Dis moi c'est quand et où la prochaine séance.")]
        public Task CKanAsync(CommandContext ctx)
        {
            var eventItem = Calendar.GetNextEvents();
            if (eventItem == null)
            {
                ctx.Message.RespondAsync("Je n'ai aucune date : https://calendar.google.com/calendar/embed?src=l0lml0k1q8ackrm1o8uscub5o8%40group.calendar.google.com&ctz=America%2FToronto");
            }

            var embed = Calendar.CreateEmbed(eventItem);
            return ctx.Message.RespondAsync(null, embed);
        }

        [Command("gf1")]
        [Description("Lance un sondage pour savoir quoi manger.")]
        public async Task GF1Async(CommandContext ctx)
        {
            var embed = Utils.CreateEmbedGF1();
            var message = await ctx.Message.RespondAsync("On mange quoi ?", embed.Reponse);
            await Utils.SendReactionsToMessage(message, embed.Emotes.ToList());
        }

        [Command("li1")]
        [Description("Lien vers le sheet d'Orga.")]
        public async Task Li1(CommandContext ctx)
        {
            await ctx.Message.RespondAsync("https://docs.google.com/spreadsheets/d/1FagGUeSOeSoIw0ANokQL46tF4MzbKeakgaWhY_RCHEw/edit?usp=sharing \nhttps://calendar.google.com/calendar/embed?src=l0lml0k1q8ackrm1o8uscub5o8%40group.calendar.google.com&ctz=America%2FToronto");
        }

        [Command("plsadd")]
        [Description("Demande l'ajout d'une fonctionnalité au créateur.")]
        public async Task PlsAdd(CommandContext ctx, [RemainingText] string text)
        {
            await (await Program.GetLoloChannel()).SendMessageAsync($"Demande d'ajout de {ctx.Message.Author} sur le channel {ctx.Message.Channel.Name} : " + text);
        }
    }

    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder EmbedBuilder;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            EmbedBuilder = new DiscordEmbedBuilder();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            EmbedBuilder.AddField(command.Name, command.Description);
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            foreach (var cmd in cmds)
            {
                EmbedBuilder.AddField(cmd.Name, cmd.Description);
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: EmbedBuilder);
        }
    }
}
