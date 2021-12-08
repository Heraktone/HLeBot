using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HLeBot
{
    public static class Utils
    {
        public class ResponseEmbedGF1
        {
            public IEnumerable<string> Emotes = Enumerable.Empty<string>();
            public DiscordEmbed Reponse;
        }
        public static ResponseEmbedGF1 CreateEmbedGF1()
        {
            var response = new ResponseEmbedGF1();
            var emotes = new Dictionary<string, string> { { "🍔", "Burger" }, { "🍕", "Pizza" }, { "🥣", "Poke" }, { "🍜", "Pho" }, { "🥬", "Aux vivres" }, { "🌶️", "Indien" }, { "🧑‍🍳", "J'ramène ma bouffe" } };
            var description = String.Join("\n", emotes.Select(kvp => kvp.Key + " - " + kvp.Value));
            response.Reponse = (new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Purple,
                Description = description
            }).Build();
            response.Emotes = emotes.Keys;
            return response;
        }

        public async static Task SendReactionsToMessage(DiscordMessage message, List<string> Emotes, bool isName = false)
        {
            foreach (var e in Emotes)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await message.CreateReactionAsync(isName ? DiscordEmoji.FromName(Program.Client, e) : DiscordEmoji.FromUnicode(e));
            }
        }
    }
}
