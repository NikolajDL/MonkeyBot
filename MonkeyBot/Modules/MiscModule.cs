﻿using Discord.Commands;
using dokas.FluentStrings;
using MonkeyBot.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MonkeyBot.Modules
{
    [Name("Misc")]
    public class MiscModule : MonkeyModuleBase
    {
        private const string lmgtfyBaseUrl = "https://lmgtfy.com/?q=";

        [Command("lmgtfy")]
        [Remarks("Generate a 'let me google that for you' link")]
        [Example("!lmgtfy Monkey Gamers")]
        public async Task LmgtfyAsync([Remainder] string searchText)
        {
            if (searchText.IsEmpty().OrWhiteSpace())
            {
                await ReplyAsync("You have to provide a search text");
                return;
            }
            var url = lmgtfyBaseUrl + HttpUtility.UrlEncode(searchText);
            await ReplyAsync(url);
        }
    }
}