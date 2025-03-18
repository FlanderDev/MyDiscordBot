using Discord;
using Discord.Commands;
using Serilog;

namespace DiscordBot.Commands;

public sealed class ManageSlashCommands : ModuleBase<SocketCommandContext>
{
    [Command("setupSlashCommands")]
    public async Task ExecuteAsync()
    {
        var botId = Context.Client.CurrentUser.Id;
        if (botId == 1302467929761120347)
            return;

        var messageAuthorId = Context.Message.Author.Id;
        if (messageAuthorId != 229720939078615040)
        {
            await Context.Message.ReplyAsync($"<@{messageAuthorId}> you are not my master, please don't even try it. Digsusting freak.");
            return;
        }

        var globalCommand = new SlashCommandBuilder();
        globalCommand.WithName("holobots");
        globalCommand.WithDescription("We bots will introduce ourselves.");

        //var abc = new SlashCommandBuilder();
        //abc.WithName("first-global-command");
        //abc.WithDescription("This is my first global slash command");

        try
        {
            var buildCommand = globalCommand.Build();
            //await guild.CreateApplicationCommandAsync(abc.Build());
            var result = await Context.Client.CreateGlobalApplicationCommandAsync(buildCommand);
            await Context.Message.ReplyAsync($"I'm done redrawing the rules of this server, boku wa aruji-sama.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not setup slash commands.");
        }
    }
}
