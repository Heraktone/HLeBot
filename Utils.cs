using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HLeBot
{
    public static class Utils
    {
        public class ResponseEmbedGF1
        {
            public IEnumerable<string> Emotes = Enumerable.Empty<string>();
            public Embed Reponse;
        }
        public static ResponseEmbedGF1 CreateEmbedGF1()
        {
            var response = new ResponseEmbedGF1();
            var emotes = new Dictionary<string, string> { { "🍔", "Burger" }, { "🍕", "Pizza" }, { "🥣", "Poke" }, { "🍜", "Pho" }, { "🥬", "Aux vivres" }, { "🌶️", "Indien" }, { "🧑‍🍳", "J'ramène ma bouffe" } };
            var description = String.Join("\n", emotes.Select(kvp => kvp.Key + " - " + kvp.Value));
            response.Reponse = (new EmbedBuilder()
            {
                Color = Color.DarkPurple,
                Description = description
            }).Build();
            response.Emotes = emotes.Keys;
            return response;
        }
    }
}
