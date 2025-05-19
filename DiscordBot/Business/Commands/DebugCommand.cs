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
    internal bool Valid => new DatabaseContext().DiscordUsers.Any(a => Context.Message.Author.Id == a.Id);

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

    [Command("debug")]
    public async Task ExecuteAsync([Remainder] string text)
    {
        try
        {
            var message = Context.Message;
            if (!Valid)
            {
                Log.Information("A non-privileged user {user} tried to use debug '{text}'.", message.Author.Id, text);
                await Context.Message.ReplyAsync("You are not a privileged user.");
                return;
            }

            Log.Information("Executing debug command '{text}'.", text);
            await using var databaseContext = new DatabaseContext();
            switch (text.ToLower())
            {
                case "ping":
                    await message.ReplyAsync("pong");
                    break;

                case "update":
                    var result = await DownloadHelper.UpdateYtDlpAsync() ? "succeeded" : "failed";
                    await message.ReplyAsync($"Update {result}");
                    break;

                case "dbReset":
                    await databaseContext.Database.EnsureDeletedAsync();
                    await databaseContext.Database.EnsureCreatedAsync();
                    await message.ReplyAsync("Recreated database.");
                    break;

                case "addPrivilegedUser":
                    var mentionedUser = message.MentionedUsers.FirstOrDefault();
                    if (mentionedUser == null)
                    {
                        var author = DiscordUser.FromSocketUser(message.Author);
                        Log.Verbose("'{author}' tried to add a user.", author);
                        await Context.Message.ReplyAsync($"You need to mention a user, to add him.");
                        return;
                    }

                    var dbUser = DiscordUser.FromSocketUser(mentionedUser);
                    databaseContext.DiscordUsers.Add(dbUser);
                    Log.Information("User '{user}' has been added.", mentionedUser);
                    await Context.Message.ReplyAsync($"Saved '{databaseContext.SaveChanges()}' changes.");
                    break;

                case "clips":
                    var audioClips = await databaseContext.AudioClips.AsNoTracking().ToArrayAsync();
                    var clipText = string.Join<AudioClip>(Environment.NewLine, audioClips);
                    await Context.Message.ReplyAsync(clipText);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error debug.");
            await Context.Message.ReplyAsync($"Digga: {ex.Message}");
        }
    }
}