using Discord;
using Discord.Commands;
using DiscordBot.Business.Helpers.Bot;
using DiscordBot.Data;
using DiscordBot.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DiscordBot.Business.Commands;

public sealed class DebugCommand : ModuleBase<SocketCommandContext>
{
    internal async Task<bool> IfInvalidCancelAsync()
    {
        var result = new DatabaseContext().DiscordUsers.Any(a => Context.Message.Author.Id == a.Id);
        if (result)
            return true;

        Log.Information("A non-privileged user {user} tried to use debug '{text}'.", Context.Message.Author.Id, Context.Message.Content);
        await Context.Message.ReplyAsync("You are not a privileged user.");
        return false;
    }

    [Command("stop")]
    public static async Task StopAsync()
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
    public async Task PingAsync()
    {
        if (!await IfInvalidCancelAsync())
            return;

        await Context.Message.ReplyAsync("pong");
    }

    [Command("debug update")]
    public async Task UpdateYouTubeDownloaderAsync()
    {
        if (!await IfInvalidCancelAsync())
            return;

        var result = await DownloadHelper.UpdateYtDlpAsync() ? "succeeded" : "failed";
        await Context.Message.ReplyAsync($"Update {result}");
    }

    [Command("debug dbReset")]
    public async Task ExecuteDbResetAsync()
    {
        if (!await IfInvalidCancelAsync())
            return;

        await using var databaseContext = new DatabaseContext();
        await databaseContext.Database.EnsureDeletedAsync();
        await databaseContext.Database.EnsureCreatedAsync();
        await Context.Message.ReplyAsync("Recreated database.");
    }

    [Command("debug addPrivilegedUser")]
    public async Task AddPrivilegedUserAsync()
    {
        if (!await IfInvalidCancelAsync())
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

    [Command("debug clips")]
    public async Task ClipsAsync()
    {
        if (!await IfInvalidCancelAsync())
            return;

        await using var databaseContext = new DatabaseContext();
        var audioClips = await databaseContext.AudioClips.AsNoTracking().ToArrayAsync();
        var clipText = string.Join<AudioClip>(Environment.NewLine, audioClips);
        await Context.Message.ReplyAsync(clipText);
    }
}