using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordBot.Business.Commands;

internal class ManualCommands(SocketUserMessage socketUserMessage)
{
    public async Task TriggerAllAsync()
    {
        await Task.WhenAll(TriggerTypingAsync(), ReplaceLinksAsync());
    }

    private async Task TriggerTypingAsync() => await socketUserMessage.Channel.TriggerTypingAsync();

    private async Task ReplaceLinksAsync()
    {
        await Task.WhenAny([
                     LinkReplacerAsync("fxtwitter.com", ["fxtwitter.com", "x.com", "twitter.com"]),
                     LinkReplacerAsync("vxreddit.com", ["fxtwitter.com", "x.com", "twitter.com"])
                     ]);

        return;
        async Task<bool> LinkReplacerAsync(string replacement, string[] toReplace)
        {
            if (socketUserMessage.CleanContent.Contains(replacement, StringComparison.OrdinalIgnoreCase))
                return false;

            var match = toReplace.FirstOrDefault(a => socketUserMessage.CleanContent.Contains(a, StringComparison.OrdinalIgnoreCase));
            if (match == null)
                return false;

            var newText = socketUserMessage.CleanContent.Replace(match, replacement);
            await socketUserMessage.ReplyAsync($"Replacing link in post from user '{socketUserMessage.Author.GlobalName}':{Environment.NewLine}{newText}");
            await socketUserMessage.DeleteAsync();
            return true;
        }
    }
}
