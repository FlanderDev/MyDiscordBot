using Discord;
using Discord.Commands;
using DiscordBot.Database;
using DiscordBot.Models.Enteties;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DiscordBot.Commands;

public sealed class DebugCommand : ModuleBase<SocketCommandContext>
{
    internal bool Valid => new DatabaseContext().DiscordUsers.Any(a => Context.Message.Author.Id == a.Id);

    [Command("debug")]
    public async Task ExecuteAsync([Remainder] string text)
    {
        try
        {
            var message = Context.Message;
            if (!Valid)
            {
                Log.Information("A non-privilaged user {user} tried to use debug '{text}'.", message.Author.Id, text);
                await Context.Message.ReplyAsync("You are not a privilaged user.");
                return;
            }

            Log.Information("Executing debug command '{text}'.", text);
            using var databasseContext = new DatabaseContext();
            switch (text)
            {
                case "dbReset":
                    databasseContext.Database.EnsureDeleted();
                    databasseContext.Database.EnsureCreated();
                    await message.ReplyAsync("Recreated database.");
                    break;

                case "addPrivalagedUser":
                    var mentionedUser = message.MentionedUsers.FirstOrDefault();
                    if (mentionedUser == null)
                    {
                        var author = DiscordUser.FromSocketUser(message.Author);
                        Log.Verbose("'{author}' tried to add a user.", author);
                        await Context.Message.ReplyAsync($"You need to mention a user, to add him.");
                        return;
                    }

                    var dbUser = DiscordUser.FromSocketUser(mentionedUser);
                    databasseContext.DiscordUsers.Add(dbUser);
                    Log.Information("User '{user}' has been added.", mentionedUser);
                    await Context.Message.ReplyAsync($"Saved '{databasseContext.SaveChanges()}' changes.");
                    break;

                case "clips":
                    var audioClips = databasseContext.AudioClips.AsNoTracking().ToArray();
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