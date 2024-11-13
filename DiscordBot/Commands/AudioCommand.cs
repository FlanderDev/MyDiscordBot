using Discord;
using Discord.Commands;
using DiscordBot.Business.Helpers;
using DiscordBot.Business.Manager;
using DiscordBot.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DiscordBot.Commands;

public class AudioCommand : ModuleBase<SocketCommandContext>
{
    [Command("ara", RunMode = RunMode.Async)]
    public async Task ExecuteAsync([Remainder] string text = "")
    {
        var botId = Context.Client.CurrentUser.Id;
        if (botId == 1302467929761120347)
            return;

        Log.Debug("Executing {method}.", nameof(ExecuteAsync));
        var voiceChannel = DiscordExtensions.GetVoiceChannel(this);
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

                var audioResource = await FileManager.GetLocalReosurceOrDownloadAsync($"ara-{num}.mp3", araUrl);
                await audioHelper.PlayAudioAsync(araUrl);

                await Task.Delay(1000);
            }
            await audioHelper.FlushAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not execute {method}.", nameof(ExecuteAsync));
        }
        finally
        {
            if (voiceChannel != null)
                await voiceChannel.DisconnectAsync();
        }
    }


    [Command("!", RunMode = RunMode.Async)]
    public async Task ExecuteClipAsync([Remainder] string text)
    {
        var context = new DatabaseContext();
        var audioClip = context.AudioClips.AsNoTracking().FirstOrDefault(f => f.CallCode.Equals(text));
        if (audioClip == null)
            return;

        var voiceChannel = (Context.Message.Author as IGuildUser)?.VoiceChannel;
        if (voiceChannel == null)
        {
            Log.Warning("Could not get voice channel.");
            return;
        }

        try
        {
            var audioClient = await voiceChannel.ConnectAsync();
            using var audioHelper = new DiscordAudioHelper(audioClient);
            await audioHelper.PlayAudioAsync(audioClip.FileName);
            await audioHelper.FlushAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error trying to play audio.");
            await Context.Message.ReplyAsync($"No '{text}', only errors.");
        }
        finally
        {
            await voiceChannel.DisconnectAsync();
        }
    }
}