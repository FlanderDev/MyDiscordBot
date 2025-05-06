using Discord;
using Discord.Commands;
using DiscordBot.Business.Helpers;
using DiscordBot.Database;
using DiscordBot.Models.Enteties;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordBot.Commands;

public sealed partial class ClipCommand : ModuleBase<SocketCommandContext>
{
    private static bool InUse = false;

    //TODO: Add logging
    [Command("!clip")]
    public async Task CreateClipAsync(params string[] data)
    {
        IUserMessage? sendMessage = null;
        try
        {
            if (InUse)
            {
                sendMessage = await Context.Message.ReplyAsync("Clip is currently in use. Please wait.");
                return;
            }

            if (data.Length < 2)
            {
                Log.Verbose("Message has not enough parts.");
                sendMessage = await Context.Message.ReplyAsync("Invalid usage, correct: 'clip {url} [callCode] {timeFrame}'.");
                return;
            }

            var clipDownloadArguments = new StringBuilder("-x --audio-format mp3 --audio-quality 0");
            if (data.Length > 2)
            {
                var match = TimeSpanMmSs().Match(data[2]);
                if (!match.Success)
                {
                    sendMessage = await Context.Message.ReplyAsync($"Invalid time-stamp.");
                    return;
                }
                clipDownloadArguments.Append($" --download-sections *{match.Value}");
            }

            var context = new DatabaseContext();
            var callCode = data[1].Trim('"');
            var callCodeAlreadyExists = await context.AudioClips.AsNoTracking().AnyAsync(a => a.CallCode.Equals(callCode));
            if (callCodeAlreadyExists)
            {
                sendMessage = await Context.Message.ReplyAsync($"The callCode '{callCode}' already exists.");
                return;
            }

            var directory = Directory.CreateDirectory("ClipDownloads");
            var fileName = $"{Context.Message.Author.GlobalName}_{Guid.NewGuid()}.mp3";
            var filePath = Path.Combine(directory.FullName, fileName);
            clipDownloadArguments.Append($" -o {filePath}");
            clipDownloadArguments.Append($" {data[0]}");


            var arguments = clipDownloadArguments.ToString();
            var message = await Context.Message.ReplyAsync($"Starting Download for '{callCode}'.");
            var downloadProcess = ExecuteDownload(arguments);
#if DEBUG
            var titleLine = await downloadProcess.StandardOutput.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(titleLine))
            {
                var errorText = await downloadProcess.StandardError.ReadToEndAsync();
                await message.ModifyAsync(mp => mp.Content = $"{errorText}{Environment.NewLine}");
                return;
            }

            await message.ModifyAsync(mp => mp.Content = $"{titleLine}{Environment.NewLine}");

            var queue = new Queue<string>(6);
            while (await downloadProcess.StandardOutput.ReadLineAsync() is { } progressLine)
            {
                if (queue.Count > 6)
                    queue.Dequeue();

                if (string.IsNullOrWhiteSpace(progressLine))
                    continue;
                
                queue.Enqueue(progressLine);
                await message.ModifyAsync(mp => mp.Content = $"{titleLine}{Environment.NewLine}{string.Join(Environment.NewLine, [.. queue])}");
            }

            await message.ModifyAsync(mp => mp.Content = $"{titleLine}{Environment.NewLine}");
#endif

            var audioClip = new AudioClip()
            {
                FileName = filePath,
                CallCode = callCode,
                DiscordUserId = Context.User.Id,
            };
            await context.AudioClips.AddAsync(audioClip);
            await context.SaveChangesAsync();

            sendMessage = await Context.Message.ReplyAsync($"Successfully added new clip, with call code '{callCode}'.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected clip error.");
            sendMessage = await Context.Message.ReplyAsync("An error occured, aborted.");
        }
        finally
        {
            await Context.Message.DeleteAsync();
            InUse = false;
            if (sendMessage != null)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    await sendMessage.DeleteAsync();
                });
        }
    }

    [Command("!", RunMode = RunMode.Async)]
    public async Task PlayClipAsync([Remainder] string text)
    {
        if (Context.Message.Author is not IGuildUser guildUser || guildUser.VoiceChannel == null)
        {
            Log.Warning("Could not get user.");
            await Context.Message.ReplyAsync($"You have to be in a voice channel.");
            return;
        }

        var context = new DatabaseContext();
        var audioClip = context.AudioClips.AsNoTracking().FirstOrDefault(f => f.CallCode.Equals(text));
        if (audioClip == null)
        {
            Log.Warning("Could not find a clip associated with {callCode} '{callCodeInput}'.", nameof(AudioClip.CallCode), text);
            await Context.Message.ReplyAsync($"Could not find a clip associated with {nameof(AudioClip.CallCode)} '{text}'.");
            return;
        }

        if (!File.Exists(audioClip.FileName))
        {
            var fullPath = Path.Combine(Environment.CurrentDirectory, audioClip.FileName);
            Log.Warning("The file '{fullPath}'w does not exist.", audioClip.FileName);
            return;
        }

        try
        {
            var audioClient = await guildUser.VoiceChannel.ConnectAsync();
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
            await guildUser.VoiceChannel.DisconnectAsync();
        }
    }

    [GeneratedRegex(@"([0-9]\d):([0-9]\d)-([0-9]\d):([0-9]\d)")]
    private static partial Regex TimeSpanMmSs();

    private static Process ExecuteDownload(string argumnets)
        => Process.Start(new ProcessStartInfo
        {
            FileName = "yt-dlp.exe",
            Arguments = argumnets,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        }) ?? throw new Exception("Could not initialize yt-dlp process.");
}