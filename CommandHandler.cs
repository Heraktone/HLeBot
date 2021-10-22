using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace HLeBot
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ckan")]
        [Summary("Dis moi c'est quand et où la prochaine séance.")]
        public Task CKanAsync()
        {
            var eventItem = Calendar.GetNextEvents();
            if (eventItem == null)
            {
                Context.Message.ReplyAsync("Je n'ai aucune date : https://calendar.google.com/calendar/embed?src=l0lml0k1q8ackrm1o8uscub5o8%40group.calendar.google.com&ctz=America%2FToronto");
            }

            var embed = Calendar.CreateEmbed(eventItem);
            return Context.Message.ReplyAsync(null, false, embed);
        }

        [Command("gf1")]
        [Summary("Lance un sondage pour savoir quoi manger.")]
        public async Task GF1Async()
        {
            var embed = Utils.CreateEmbedGF1();
            var message = await Context.Message.ReplyAsync("On mange quoi ?", false, embed.Reponse);
            await message.AddReactionsAsync(embed.Emotes.Select(e => new Emoji(e)).ToArray());
        }

        [Command("li1")]
        [Summary("Lien vers le sheet d'Orga.")]
        public async Task Li1()
        {
            await Context.Message.ReplyAsync("https://docs.google.com/spreadsheets/d/1FagGUeSOeSoIw0ANokQL46tF4MzbKeakgaWhY_RCHEw/edit?usp=sharing");
        }

        [Command("plsadd")]
        [Summary("Demande l'ajout d'une fonctionnalité au créateur.")]
        public async Task PlsAdd([Remainder] string text)
        {
            await (await Program.GetLoloChannel()).SendMessageAsync($"Demande d'ajout de {Context.Message.Author} sur le channel {Context.Message.Channel.Name} : " + text);
        }

        [Command("help")]
        [Summary("Donne la liste des commandes.")]
        public async Task HelpAsync()
        {
            string helpCommand = "";

            foreach (var module in Program.Commands.Modules)
            {
                foreach (var cmd in module.Commands)
                {
                    helpCommand += $"`!{cmd.Aliases.First()}` : {cmd.Summary}\n";
                }
            }

            await Context.Message.ReplyAsync(helpCommand);
        }
    }
}
