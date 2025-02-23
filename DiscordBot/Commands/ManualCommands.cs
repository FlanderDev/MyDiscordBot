using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands;

internal class ManualCommands(SocketUserMessage socketUserMessage)
{
    public async Task TriggerAllAsync()
    {
        await Task.WhenAll(ClaimAsync(), FuckTwitterAsync());
    }

    public async Task ClaimAsync()
    {
        if (!socketUserMessage.CleanContent.StartsWith(".claim", StringComparison.OrdinalIgnoreCase))
            return;

        if (socketUserMessage.ReferencedMessage == null)
            return;

        var referenceMessage = await socketUserMessage.Channel.GetMessageAsync(socketUserMessage.ReferencedMessage.Id);
        var component = referenceMessage.Components.FirstOrDefault();
        if (component is not ActionRowComponent actionRowComponent)
            return;

        var messageComponent = actionRowComponent.Components.FirstOrDefault();
        if (messageComponent is not ButtonComponent buttonComponent)
            return;

        await referenceMessage.AddReactionAsync(buttonComponent.Emote);
    }

    public async Task FuckTwitterAsync()
    {
        if (new string[] { "x.com", "twitter.com", "fxtwitter.com" }.Any(a => socketUserMessage.CleanContent.Contains(a, StringComparison.OrdinalIgnoreCase)))
        {
            await socketUserMessage.ReplyAsync("https://tenor.com/view/shitter-alert-cake-gif-19194039");
            await socketUserMessage.Author.SendMessageAsync($"We don't support twitter because of it's CEO. Inform yourself:{Environment.NewLine}https://en.wikipedia.org/wiki/Elon_Musk#Accusations_of_antisemitism");
            await socketUserMessage.DeleteAsync();
        }
    }
}
