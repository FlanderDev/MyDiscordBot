using Discord;
using Discord.Commands;
using DiscordBot.Business.Helpers;
using DiscordBot.Data;
using DiscordBot.Models.Enteties;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.RegularExpressions;

namespace DiscordBot.Business.Commands;

public sealed partial class ClipCommand : ModuleBase<SocketCommandContext>
{
    //TODO: Add logging
    [Command("!clip")]
    public async Task CreateClipAsync(params string[] data)
    {
        try
        {
            string? url = null;
            string? callCode = null;
            TimeSpan? start = null;
            TimeSpan? end = null;

            if (data.Length == 0)
            {
                Log.Verbose("Message has not enough parts.");
                await Context.Message.ReplyAsync("Invalid usage, correct: 'clip {url} [callCode] {from} {to}'.");
                return;
            }

            if (data.Length > 0)
                url = Uri.IsWellFormedUriString(data[0], UriKind.Absolute) ? data[0] : null;
            if (data.Length > 1)
                callCode = data[1];
            if (data.Length > 2)
                start = TimeSpan.TryParse(data[2], out var value) ? value : null;
            if (data.Length > 3)
                end = TimeSpan.TryParse(data[3], out var value) ? value : null;

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(callCode))
            {
                Log.Verbose("Call code or url is invalid: '{callCode}' '{url}'", callCode, url);
                await Context.Message.ReplyAsync("Invalid usage, correct: 'clip {url} {callCode} [from] [to]'.");
                return;
            }

            var context = new DatabaseContext();
            var existingCallCode = await context.AudioClips.FirstOrDefaultAsync(a => a.CallCode.Equals(callCode));
            if (existingCallCode != null)
            {
                if (File.Exists(existingCallCode.FileName))
                {
                    await Context.Message.ReplyAsync($"The callCode '{callCode}' already exists.");
                    return;
                }

                await Context.Message.ReplyAsync($"The callCode '{callCode}' already exists, but the file could not be found. Freeing call code.");
                context.AudioClips.Remove(existingCallCode);
                await context.SaveChangesAsync();
            }

            var startText = start?.ToString("g") ?? "start";
            var endText = end?.ToString("g") ?? "end";
            await Context.Message.ReplyAsync($"Starting Download for '{callCode}' from {startText} to {endText}");

            var filePath = await DownloadHelper.DownloadYouTubeMediaAsync(false, data[0], Context.User.GlobalName, start, end);
            if (filePath == null)
            {
                await Context.Message.ReplyAsync("Could not create clip.");
                return;
            }

            var audioClip = new AudioClip
            {
                FileName = filePath,
                CallCode = callCode,
                DiscordUserId = Context.User.Id,
            };
            context.AudioClips.Add(audioClip);
            await context.SaveChangesAsync();

            await Context.Message.ReplyAsync($"Successfully added new clip, with call code '{callCode}'.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected clip error.");
            await Context.Message.ReplyAsync("An error occured, aborted.");
        }
    }

    [Command("!", RunMode = RunMode.Async)]
    public async Task PlayClipAsync([Remainder] string text)
    {
        if (Context.Message.Author is not IGuildUser guildUser || guildUser.VoiceChannel == null)
        {
            await Context.Message.ReplyAsync("You have to be in a voice channel.");
            return;
        }

        try
        {
            var context = new DatabaseContext();
            var audioClip = context.AudioClips.AsNoTracking().FirstOrDefault(f => f.CallCode.Equals(text));
            if (audioClip == null)
            {
                Log.Warning("Could not find a clip associated with {callCode} '{callCodeInput}'.", nameof(AudioClip.CallCode), text);
                await Context.Message.ReplyAsync($"Could not find a clip associated with {nameof(AudioClip.CallCode)} '{text}'.");
                return;
            }

            var data = new DirectoryInfo(Environment.CurrentDirectory);
            var files = data.GetFiles();
            if (!File.Exists(audioClip.FileName))
            {
                var fullPath = Path.Combine(Environment.CurrentDirectory, audioClip.FileName);
                Log.Warning("The file '{fullPath}'w does not exist.", audioClip.FileName);
                await Context.Message.ReplyAsync($"No associated audio could be found for the valid callCode '{audioClip.CallCode}', freeing callCode.");
                context.AudioClips.Remove(audioClip);
                await context.SaveChangesAsync();
                return;
            }

            var audioClient = await guildUser.VoiceChannel.ConnectAsync();
            using var audioHelper = new DiscordAudioHelper(audioClient);
            await audioHelper.PlayAudioAsync(audioClip.FileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error trying to play audio.");
            await Context.Message.ReplyAsync($"No '{text}', only errors.");
        }
        finally
        {
            await guildUser.VoiceChannel.DisconnectAsync();
        }
    }

    [GeneratedRegex(@"([0-9]\d):([0-9]\d)-([0-9]\d):([0-9]\d)")]
    private static partial Regex TimeSpanMmSs();
}