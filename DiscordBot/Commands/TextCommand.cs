using Discord.Commands;
using DiscordBot.Business.Helpers;
using Serilog;

namespace DiscordBot.Commands;

public class TextCommand : ModuleBase<SocketCommandContext>
{
    [Command("HoloBots")]
    public async Task HoloBotsAsync()
    {
        try
        {
            Log.Debug("Executing HoloBots.");

            var botId = Context.Client.CurrentUser.Id;

            if (botId == 1302467929761120347) // Kumo
            {
                var result = await Danbooru.GetRandomImageByTagAsync("+shiraori", "+solo");
                if (result == null)
                    await Context.Channel.SendMessageAsync("YOU WORM! You won't receive ANY images from me!");
                else
                    await Context.Channel.SendMessageAsync(result);
            }
            else if (botId == 995955672934006784) //Ina
            {
                var result = await Danbooru.GetRandomImageByTagAsync("+ninomae_ina'nis", "+solo");
                if (result == null)
                    await Context.Channel.SendMessageAsync("Sowy Tako <3, I, couldn't fetch any images, maybe next time^^");
                else
                    await Context.Channel.SendMessageAsync(result);
            }
            else
                throw new Exception($"Bot's don't match any id: {botId}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not execute HoloBots.");
        }
    }

    private static int PunCounter = 0;
    [Command("InaPun")]
    public async Task InaPanAsync()
    {
        try
        {
            Log.Debug("Executing InaPun.");

            if (Context.Client.CurrentUser.Id != 995955672934006784) //Ina
                return;

            var result = await Danbooru.GetRandomImageByTagAsync(PunCounter++, "+ninomae_ina'nis", "+pun");
            if (result == null)
            {
                PunCounter = 0;
                await Context.Channel.SendMessageAsync("I'm not felling *pan*Tastic.");
            }
            else
                await Context.Channel.SendMessageAsync(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not execute InaPun.");
        }
    }
}
