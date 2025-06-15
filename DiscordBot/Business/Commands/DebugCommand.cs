using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Business.Bots;
using DiscordBot.Business.Helpers.Bot;
using DiscordBot.Data;
using DiscordBot.Models.Entities;
using DiscordBot.Models.Internal.Configs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace DiscordBot.Business.Commands;

public sealed class DebugCommand(
    [FromServices] IOptions<Configuration> options,
    [FromServices] DiscordNet discordNet) : ModuleBase<SocketCommandContext>
{
    internal async Task<bool> ReplyWithFailureIfUnprivilegedAsync()
    {
        var currentUserId = Context.Message.Author.Id;
        var defaultAdminIds = options.Value.Discord.IdOfAdmins;

        var result = (defaultAdminIds.Count != 0 && defaultAdminIds.Contains(currentUserId))
                      || new DatabaseContext().DiscordUsers.Any(a => a.Administrator && a.Id == currentUserId);
        if (result)
            return true;

        Log.Information("A non-privileged user {user} tried to use debug '{text}'.", Context.Message.Author.Id, Context.Message.Content);
        await Context.Message.ReplyAsync("You are not a privileged user.");
        return false;
    }

    #region Unprivileged
    [Command("stop")]
    public async Task StopAsync()
    {
        try
        {
            if (!Common.CancellationTokenSource.IsCancellationRequested)
            {
                Log.Verbose("Canceling current action.");
                await Common.CancellationTokenSource.CancelAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while stopping");
        }
    }

    [Command("debug ping")]
    public async Task PingAsync() => await Context.Message.ReplyAsync("pong");

    [Command("debug clips")]
    public async Task ClipsAsync()
    {
        await using var databaseContext = new DatabaseContext();
        var audioClips = await databaseContext.AudioClips.AsNoTracking().ToArrayAsync();
        var clipText = string.Join<AudioClip>(Environment.NewLine, audioClips);
        await Context.Message.ReplyAsync(clipText);
    }
    #endregion



    #region Privileged
    [Command("playAudioOnAllServers")]
    public async Task PlayAudioOnAllServersAsync()
    {
        try
        {
            if (!await ReplyWithFailureIfUnprivilegedAsync())
                return;


            using var context = new DatabaseContext();
            var audioClip = context.AudioClips.AsNoTracking().FirstOrDefault();
            if (audioClip == null)
                return;

            var guilds = discordNet.DiscordSocketClient.Guilds.ToList();
            var tasks = guilds.Where(f => f.Name.Equals("Nerds & Weebs", StringComparison.OrdinalIgnoreCase))
                .Select(s => PlayAudioOnServerAsync(s))
                .ToList();

            await Task.WhenAll(tasks);
            Log.Information("Done playing on all servers {guildAmount}.", guilds.Count);
            return;

            async Task<bool> PlayAudioOnServerAsync(SocketGuild guild)
            {
                var voiceChannel = guild.VoiceChannels.First();
                if (voiceChannel == null)
                    return false;

                Log.Information("Connecting to '{channel}' in '{guild}'.", voiceChannel.Name, guild.Name);
                var audioChannel = await voiceChannel.ConnectAsync();

                Log.Information("Playing in '{channel}' in '{guild}'.", voiceChannel.Name, voiceChannel.Name);
                _ = audioChannel.PlayAudioAsync(audioClip.FilePath);

                return true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error playing on all servers.");
        }
    }


    [Command("debug update")]
    public async Task UpdateYouTubeDownloaderAsync()
    {
        if (!await ReplyWithFailureIfUnprivilegedAsync())
            return;

        var result = await DownloadHelper.UpdateYtDlpAsync() ? "succeeded" : "failed";
        await Context.Message.ReplyAsync($"Update {result}");
    }

    [Command("debug dbReset")]
    public async Task ExecuteDbResetAsync()
    {
        if (!await ReplyWithFailureIfUnprivilegedAsync())
            return;

        await using var databaseContext = new DatabaseContext();
        await databaseContext.Database.EnsureDeletedAsync();
        await databaseContext.Database.EnsureCreatedAsync();
        await Context.Message.ReplyAsync("Recreated database.");
    }

    [Command("debug addPrivilegedUser")]
    public async Task AddPrivilegedUserAsync()
    {
        if (!await ReplyWithFailureIfUnprivilegedAsync())
            return;

        await using var databaseContext = new DatabaseContext();
        var mentionedUser = Context.Message.MentionedUsers.FirstOrDefault();
        if (mentionedUser == null)
        {
            var author = DiscordUser.FromSocketUser(Context.Message.Author);
            Log.Verbose("'{author}' tried to add a user.", author);
            await Context.Message.ReplyAsync($"You need to mention a user, to add him.");
            return;
        }

        var dbUser = DiscordUser.FromSocketUser(mentionedUser);
        databaseContext.DiscordUsers.Add(dbUser);
        Log.Information("User '{user}' has been added.", mentionedUser);
        await Context.Message.ReplyAsync($"Saved '{await databaseContext.SaveChangesAsync()}' changes.");
    }
    #endregion
}