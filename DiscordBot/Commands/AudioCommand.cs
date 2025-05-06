using Discord.Commands;
using DiscordBot.Business.Helpers;
using DiscordBot.Business.Manager;
using Serilog;

namespace DiscordBot.Commands;

public sealed class AudioCommand : ModuleBase<SocketCommandContext>
{
    [Command("araAll", RunMode = RunMode.Async)]
    public async Task AraAllAsync()
    {
        var voiceChannel = this.GetVoiceChannel();
        if (voiceChannel == null)
        {
            await Context.Channel.SendMessageAsync("Could not join your voice-channel, gomenasorry.");
            return;
        }

        var audioClient = await voiceChannel.ConnectAsync();
        using var audioHelper = new DiscordAudioHelper(audioClient);

        for (var i = 12; i < 600; i++)
        {
            var araUrl = $"https://faunaraara.com/sounds/ara-{i}.mp3";
            var audioResource = await FileManager.GetLocalResourceOrDownloadAsync($"ara-{i}.mp3", araUrl);
            if (audioResource == null)
            {
                await Context.Channel.SendMessageAsync("Your not in a VC, gomenasorry.");
                continue;
            }
            await audioHelper.PlayAudioAsync(audioResource);

            await Task.Delay(200);
        }
        await audioHelper.FlushAsync();
    }

    [Command("ara", RunMode = RunMode.Async)]
    public async Task AraAsync([Remainder] string text = "")
    {
        var botId = Context.Client.CurrentUser.Id;
        if (botId == 1302467929761120347)
            return;

        Log.Debug("Executing {method}.", nameof(AraAllAsync));
        var voiceChannel = this.GetVoiceChannel();
        try
        {
            var split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var id = split.Length > 0 ? split[0] : "-1";
            var araId = int.TryParse(id.Trim(), out var idTemp) ? idTemp : -1;

            var number = split.Length > 1 ? split[1] : "1";
            var aras = int.TryParse(number.Trim(), out var numTemp) ? numTemp : 3;

            Console.WriteLine($"[{DateTime.Now:g}] {Context.Guild.Name}: {Context.User.GlobalName} '{number}'={aras}");

            if (voiceChannel == null)
            {
                await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            var audioClient = await voiceChannel.ConnectAsync();
            using var audioHelper = new DiscordAudioHelper(audioClient);

            for (var i = 0; i < aras; i++)
            {
                var num = araId == -1 ? Random.Shared.Next(10, 510) : araId;
                //await audioHelper.PlayAudioAsync(@$"E:\aras\ara-{num}.mp3");
                var araUrl = @$"https://faunaraara.com/sounds/ara-{num}.mp3";
                if (araId == -1)
                    _ = Context.Channel.SendMessageAsync($"[Ara-{num}]({araUrl})");

                var audioResource = await FileManager.GetLocalResourceOrDownloadAsync($"ara-{num}.mp3", araUrl);
                await audioHelper.PlayAudioAsync(araUrl);

                await Task.Delay(1000);
            }
            await audioHelper.FlushAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not execute {method}.", nameof(AraAllAsync));
        }
        finally
        {
            if (voiceChannel != null)
                await voiceChannel.DisconnectAsync();
        }
    }
}