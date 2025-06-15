using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Discord;
using Discord.Commands;
using DiscordBot.Business.Helpers.Bot;
using DiscordBot.Models.Entities;
using Serilog;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.HttpLogging;
using RestSharp.Extensions;

namespace DiscordBot.Business.Commands;

public sealed partial class ClipCommand : ModuleBase<SocketCommandContext>
{
    //TODO: Add more logging
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

            var callCodeProperty = typeof(AudioClip).GetProperty(nameof(AudioClip.CallCode));
            if (callCodeProperty == null) // If this branch is ever hit; I'll be sobbing on the floor.
            {
                Log.Fatal("Could not get property '{callCodePropertyName}'.", nameof(AudioClip.CallCode));
                await Context.Message.ReplyAsync("Internal program error please inform the developer.");
                return;
            }

            var callCodeMaxLength = callCodeProperty.GetCustomAttribute<MaxLengthAttribute>();
            if (callCodeMaxLength?.Length < callCode.Length)
            {
                Log.Verbose("The call code '{callCode}' is '{callCodeLength}' long, which is more then the allowed '{callCodeMaxLength}'.", callCode, callCode.Length, callCodeMaxLength);
                await Context.Message.ReplyAsync("Internal program error please inform the developer.");
                return;
            }

            if (await ClipHelper.DoesCallCodeExistAsync(callCode))
            {
                Log.Verbose("The callCode '{callCode}' already exists.", callCode);
                await Context.Message.ReplyAsync($"The callCode '{callCode}' already exists.");
                return;
            }

            var startText = start?.ToString("g") ?? "start";
            var endText = end?.ToString("g") ?? "end";
            await Context.Message.ReplyAsync($"Starting Download for '{callCode}' from {startText} to {endText}");

            var filePath = await DownloadHelper.DownloadYouTubeMediaAsync(false, data[0], Context.User.GlobalName, start, end);
            if (filePath == null)
            {
                await Context.Message.ReplyAsync("Could not download the media required for the clip.");
                return;
            }

            var audioClip = new AudioClip
            {
                FilePath = filePath,
                CallCode = callCode,
                DiscordUserId = Context.User.Id,
            };

            if (await ClipHelper.AddNewClipAsync(audioClip))
                await Context.Message.ReplyAsync($"Successfully added new clip, with call code '{callCode}'.");
            else
                await Context.Message.ReplyAsync($"Could not add new clip, with call code; '{callCode}'.");

            Log.Information("User '{userName}' added clip '{callcode}' with url '{url}'.", Context.Message.Author.GlobalName, callCode, url);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected clip error.");
            await Context.Message.ReplyAsync("An error occured, aborted.");
        }
    }

    [Command("!", RunMode = RunMode.Async)]
    public async Task PlayClipAsync([Remainder] string callCode)
    {
        if (Context.Message.Author is not IGuildUser guildUser || guildUser.VoiceChannel == null)
        {
            await Context.Message.ReplyAsync("You have to be in a voice channel.");
            return;
        }

        try
        {
            var audioClip = await ClipHelper.GetValidateCallCodeAsync(callCode);
            if (audioClip == null)
            {
                Log.Warning("Could not find a clip associated with {callCode} '{callCodeInput}'.", nameof(AudioClip.CallCode), callCode);
                await Context.Message.ReplyAsync($"Could not find a clip associated with {nameof(AudioClip.CallCode)} '{callCode}'.");
                return;
            }

            var audioClient = await guildUser.VoiceChannel.ConnectAsync();
            await audioClient.PlayAudioAsync(audioClip.FilePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error trying to play audio.");
            await Context.Message.ReplyAsync($"No '{callCode}', only errors.");
        }
        finally
        {
            await guildUser.VoiceChannel.DisconnectAsync();
        }
    }

    [GeneratedRegex(@"([0-9]\d):([0-9]\d)-([0-9]\d):([0-9]\d)")]
    private static partial Regex TimeSpanMmSs();
}