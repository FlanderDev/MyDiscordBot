using Discord;
using Discord.Commands;
using DiscordBot.Business.Helpers;

namespace DiscordBot.Commands;

public class AudioCommand : ModuleBase<SocketCommandContext>
{
    [Command("ara ara", RunMode = RunMode.Async)]
    public async Task ExecuteAsync([Remainder] string number)
    {
        var voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;
        try
        {
            var aras = int.TryParse(number.Trim(), out var temp) ? (temp > 0 ? temp : 1) : 3;
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
                var num = Random.Shared.Next(10, 510);
                await audioHelper.PlayAudioAsync(@$"https://faunaraara.com/sounds/ara-{num}.mp3");
            }
            await audioHelper.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (voiceChannel != null)
                await voiceChannel.DisconnectAsync();
        }
    }
}