using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HLeBot
{
    public class Commands : ApplicationCommandModule
    {
        [SlashCommand("ckan", "Dis moi c'est quand et où la prochaine séance.")]
        public async Task CKanAsync(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var eventItem = Calendar.GetNextEvent();
            if (eventItem == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Je n'ai aucune date : https://calendar.google.com/calendar/embed?src=l0lml0k1q8ackrm1o8uscub5o8%40group.calendar.google.com&ctz=America%2FToronto"));
            }

            var embed = Calendar.CreateEmbed(eventItem);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("ckanchez", "Dis moi c'est quand et où la prochaine séance chez intel.")]
        public async Task CKanChezAsync(InteractionContext ctx, [Option("Prenom", "prenom")] string prenom)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var events = Calendar.GetNextEvents(200);
            var eventItem = events.Items.FirstOrDefault(e => e.Location.Contains(prenom, System.StringComparison.OrdinalIgnoreCase));
            if (eventItem == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Je n'ai aucune date : https://calendar.google.com/calendar/embed?src=l0lml0k1q8ackrm1o8uscub5o8%40group.calendar.google.com&ctz=America%2FToronto"));
            }

            var embed = Calendar.CreateEmbed(eventItem);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("gf1", "Lance un sondage pour savoir quoi manger.")]
        public async Task GF1Async(InteractionContext ctx)
        {
            var embed = Utils.CreateEmbedGF1();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("On mange quoi ?").AddEmbed(embed.Reponse));
            
            await Utils.SendReactionsToMessage(await ctx.GetOriginalResponseAsync(), embed.Emotes.ToList());
        }

        [SlashCommand("li1", "Lien vers le sheet d'Orga.")]
        public async Task Li1(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("https://docs.google.com/spreadsheets/d/1FagGUeSOeSoIw0ANokQL46tF4MzbKeakgaWhY_RCHEw/edit?usp=sharing \nhttps://calendar.google.com/calendar/embed?src=l0lml0k1q8ackrm1o8uscub5o8%40group.calendar.google.com&ctz=America%2FToronto"));
        }

        [SlashCommand("spam", "Envoie un message a travers le bot, oui ça sert a rien, mais voila.")]
        public async Task Spam(InteractionContext ctx, [Option("pseudo", "Pseudo")] DiscordUser userName, [Option("message", "Message à envoyer")] string message = "")
        {
            await (await ctx.Guild.GetMemberAsync(userName.Id)).SendMessageAsync($"{ctx.Member.Username} : {(string.IsNullOrWhiteSpace(message) ? "Graou." : message)}");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("C'est fait voila, bisous"));
            await ctx.DeleteResponseAsync();
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Spam")]
        public async Task SpamUser(ContextMenuContext ctx) 
        {
            await ctx.TargetMember.SendMessageAsync($"{ctx.Member.Username} : Graou.");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("C'est fait voila, bisous"));
            await ctx.DeleteResponseAsync();
        }

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Buy as NFT")]
        public async Task BuyMessage(ContextMenuContext ctx)
        {
            if (Sheet.BuyNFT(ctx.TargetMessage.Id.ToString(), ctx.User.Username))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Tu as acheté {ctx.TargetMessage.Id}, il est a toi maintenant, super."));
            } else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Zut, {ctx.TargetMessage.Id} appartient déja a quelqu'un d'autre, dommage, peut être qu'un jour tu pourras lui acheter."));
            }
        }

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Get Owner NFT")]
        public async Task GetOwnerMessage(ContextMenuContext ctx)
        {
            var user = Sheet.GetNFT(ctx.TargetMessage.Id.ToString());
            if (string.IsNullOrWhiteSpace(user))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{ctx.TargetMessage.Id}, n'appartient à personne, vite achète le."));
            } else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{ctx.TargetMessage.Id}, appartient à {user}."));
            }
        }


        [SlashCommand("plsadd", "Demande l'ajout d'une fonctionnalité au créateur.")]
        public async Task PlsAdd(InteractionContext ctx, [Option("fonctionnalite", "Fonctionnalité à ajouter")] string text)
        {
            await (await Program.GetLoloChannel()).SendMessageAsync($"Demande d'ajout de {ctx.Member.Username} sur le channel {ctx.Channel.Name} : " + text);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("C'est fait voila, bisous"));
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("paul", "Fait un sondage classique.")]
        public async Task Poll(InteractionContext ctx, [Option("question", "question")] string question, [Option("option_1", "option_1")] string option_1 = null, [Option("option_2", "option_2")] string option_2 = null, [Option("option_3", "option_3")] string option_3 = null, [Option("option_4", "option_4")] string option_4 = null, [Option("option_5", "option_5")] string option_5 = null, [Option("option_6", "option_6")] string option_6 = null, [Option("option_7", "option_7")] string option_7 = null, [Option("option_8", "option_8")] string option_8 = null, [Option("option_9", "option_9")] string option_9 = null, [Option("option_10", "option_10")] string option_10 = null, [Option("option_11", "option_11")] string option_11 = null, [Option("option_12", "option_12")] string option_12 = null, [Option("option_13", "option_13")] string option_13 = null, [Option("option_14", "option_14")] string option_14 = null, [Option("option_15", "option_15")] string option_15 = null, [Option("option_16", "option_16")] string option_16 = null, [Option("option_17", "option_17")] string option_17 = null, [Option("option_18", "option_18")] string option_18 = null, [Option("option_19", "option_19")] string option_19 = null, [Option("option_20", "option_20")] string option_20 = null)
        {
            var options = new List<string>() { option_1, option_2, option_3, option_4, option_5, option_6, option_7, option_8, option_9, option_10, option_11, option_12, option_13, option_14, option_15, option_16, option_17, option_18, option_19, option_20 };
            options = options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
            if (options.Count > 1 && options.Count < 22)
            {
                var embed = (new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Description = string.Join("\n", options.Select((o, i)=> $":regional_indicator_{(char)(i+97)}: {o}"))
                }).Build();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($":bar_chart: {question}").AddEmbed(embed));
                await Utils.SendReactionsToMessage(await ctx.GetOriginalResponseAsync(), options.Select((o, i) => $":regional_indicator_{(char)(i + 97)}:").ToList(), true);
            }
        }

        [SlashCommand("cpaul", "Lance un vote de Condorcet. (Non implémenté)")]
        public async Task CPoll(InteractionContext ctx, [Option("question", "question")] string question, [Option("option_1", "option_1")] string option_1 = null, [Option("option_2", "option_2")] string option_2 = null, [Option("option_3", "option_3")] string option_3 = null, [Option("option_4", "option_4")] string option_4 = null, [Option("option_5", "option_5")] string option_5 = null, [Option("option_6", "option_6")] string option_6 = null, [Option("option_7", "option_7")] string option_7 = null, [Option("option_8", "option_8")] string option_8 = null, [Option("option_9", "option_9")] string option_9 = null, [Option("option_10", "option_10")] string option_10 = null, [Option("option_11", "option_11")] string option_11 = null, [Option("option_12", "option_12")] string option_12 = null, [Option("option_13", "option_13")] string option_13 = null, [Option("option_14", "option_14")] string option_14 = null, [Option("option_15", "option_15")] string option_15 = null, [Option("option_16", "option_16")] string option_16 = null, [Option("option_17", "option_17")] string option_17 = null, [Option("option_18", "option_18")] string option_18 = null, [Option("option_19", "option_19")] string option_19 = null, [Option("option_20", "option_20")] string option_20 = null)
        {
            var options = new List<string>() { option_1, option_2, option_3, option_4, option_5, option_6, option_7, option_8, option_9, option_10, option_11, option_12, option_13, option_14, option_15, option_16, option_17, option_18, option_19, option_20 };
            options = options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();
            if (options.Count > 1 && options.Count < 22)
            {
                var embed = (new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Description = string.Join("\n", options.Select((o, i) => $":regional_indicator_{(char)(i + 97)}: {o}"))
                }).Build();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($":bar_chart: {question}").AddEmbed(embed));
                await (await ctx.GetOriginalResponseAsync()).CreateThreadAsync(question, DSharpPlus.AutoArchiveDuration.Hour);
            }
        }
    }
}
