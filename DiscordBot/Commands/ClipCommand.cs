using Discord;
using Discord.Commands;
using DiscordBot.Database;
using DiscordBot.Models.Database;
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
    [Command("clip")]
    public async Task CreateAsync(params string[] data)
    {
        try
        {
            if (InUse)
            {
                await Context.Message.ReplyAsync("Clip is currently in use. Please wait.");
                return;
            }

            if (data.Length < 1)
            {
                await Context.Message.ReplyAsync($"Invalid usage, add an URL.");
                return;
            }

            var stringBuilder = new StringBuilder("-x --audio-format mp3 --audio-quality 0");
            if (data.Length > 2) //The timespan
            {
                var match = MyRegex().Match(data[1]);
                if (!match.Success)
                {
                    await Context.Message.ReplyAsync($"Invalid time-stamp.");
                    return;
                }

                stringBuilder.Append($" --download-sections *{match.Value}");
            }

            var context = new DatabaseContext();
            var callCode = data[2].Trim('"');
            var callCodeAlreadyExists = await context.AudioClips.AsNoTracking().AnyAsync(a => a.CallCode.Equals(callCode));
            if (callCodeAlreadyExists)
            {
                await Context.Message.ReplyAsync($"The callCode '{callCode}' already exists.");
                return;
            }

            var directory = Directory.CreateDirectory("ClipDownloads");
            var fileName = $"{Context.Message.Author.GlobalName}_{Guid.NewGuid()}.mp3";
            var filePath = Path.Combine(directory.FullName, fileName);
            stringBuilder.Append($" -o {filePath}");
            stringBuilder.Append($" {data[0]}");


            var arguments = stringBuilder.ToString();
            var downloadProcess = ExecuteDownload(arguments);

#if DEBUG
            var stringBuidlerOutput = new StringBuilder();
            stringBuidlerOutput.AppendLine(downloadProcess.StandardOutput.ReadLine());
            var message = await Context.Message.ReplyAsync(stringBuidlerOutput.ToString());
            while (downloadProcess.StandardOutput.ReadLine() is string line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    stringBuidlerOutput.AppendLine(line);
                    await message.ModifyAsync(mp => mp.Content = stringBuidlerOutput.ToString());
                }
            }
#endif

            var audioClip = new AudioClip()
            {
                FileName = fileName,
                CallCode = callCode,
                DiscordUserId = Context.User.Id,
            };
            await context.AudioClips.AddAsync(audioClip);
            await context.SaveChangesAsync();

            await Context.Message.ReplyAsync($"Successfully added new clip, with call code '{callCode}'.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected clip error.");
            await Context.Message.ReplyAsync("An error occured, aborted.");
        }
        finally
        {
            InUse = false;
        }
    }

    [GeneratedRegex(@"([0-5]\d):([0-5]\d)-([0-5]\d):([0-5]\d)")]
    private static partial Regex MyRegex();

    private static Process ExecuteDownload(string argumnets)
        => Process.Start(new ProcessStartInfo
        {
            FileName = @"""E:\Downloads\ytdlp-interface\yt-dlp.exe""",
            Arguments = argumnets,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        }) ?? throw new Exception("Could not initialize ffmpeg process.");
}