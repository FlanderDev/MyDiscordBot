using Discord.Commands;
using DiscordBot.Business.Helpers;
using Serilog;

namespace DiscordBot.Business.Commands;

public sealed class TextCommand : ModuleBase<SocketCommandContext>
{
    [Command("holobots")]
    public async Task HoloBotsAsync()
    {
        try
        {
            Log.Debug("Executing HoloBots.");

            var botId = Context.Client.CurrentUser.Id;

            if (botId == 1302467929761120347) // Kumo
            {
                var (imageUrl, imageIndex) = await DanbooruHelper.GetRandomImageByTagAsync("+shiraori", "+solo");
                if (imageUrl == null)
                    await Context.Channel.SendMessageAsync("YOU WORM! You won't receive ANY images from me!");
                else
                    await Context.Channel.SendMessageAsync(imageUrl);
            }
            else if (botId == 995955672934006784) //Ina
            {
                var (imageUrl, imageIndex) = await DanbooruHelper.GetRandomImageByTagAsync("+ninomae_ina'nis", "+solo");
                if (imageUrl == null)
                    await Context.Channel.SendMessageAsync("Sowy Tako <3, I, couldn't fetch any images, maybe next time^^");
                else
                    await Context.Channel.SendMessageAsync(imageUrl);
            }
            else
                throw new Exception($"Bot's don't match any id: {botId}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not execute HoloBots.");
        }
    }

    private static int PunCounter = -1;
    [Command("InaPun")]
    public async Task InaPanAsync()
    {
        try
        {
            Log.Debug("Executing InaPun.");

            if (Context.Client.CurrentUser.Id != 995955672934006784) //Ina
                return;

            var (imageUrl, imageIndex) = await DanbooruHelper.GetRandomImageByTagAsync(PunCounter, "+ninomae_ina'nis", "+pun");
            if (imageUrl == null)
            {
                PunCounter = imageIndex++;
                await Context.Channel.SendMessageAsync("I'm not felling *pan*Tastic.");
            }
            else
                await Context.Channel.SendMessageAsync(imageUrl);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not execute InaPun.");
        }
    }
}
