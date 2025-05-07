using Discord;
using Discord.WebSocket;

namespace DiscordBot.Business.Commands;

internal class ManualCommands(SocketUserMessage socketUserMessage)
{
    public async Task TriggerAllAsync()
    {
        await Task.WhenAll(ClaimAsync(), FuckTwitterAsync());
    }

    private async Task ClaimAsync()
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

    private async Task FuckTwitterAsync()
    {
        string[] twitterUrls = ["fxtwitter.com", "x.com", "twitter.com"];
        var match = twitterUrls.FirstOrDefault(a => socketUserMessage.CleanContent.Contains(a, StringComparison.OrdinalIgnoreCase));
        if (match == null)
            return;

        var newText = socketUserMessage.CleanContent.Replace(match, "fxtwitter.com");
        await socketUserMessage.ReplyAsync($"Replacing link in post from user '{socketUserMessage.Author.GlobalName}':{Environment.NewLine}{newText}");
        await socketUserMessage.DeleteAsync();
    }
}
