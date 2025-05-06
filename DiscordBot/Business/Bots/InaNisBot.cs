using System.Net.Sockets;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;
using DiscordBot.Business.Commands;

namespace DiscordBot.Business.Bots;

public sealed class InaNisBot
{
    internal DiscordSocketClient DiscordSocketClient { get; }
    internal CommandService Commands { get; }
    internal IServiceProvider ServiceProvider { get; private set; }
    internal char? Prefix { get; private set; }

    public InaNisBot(IServiceProvider serviceProvider, char? prefix = null)
    {
        ServiceProvider = serviceProvider;
        Prefix = prefix;

        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        DiscordSocketClient = new DiscordSocketClient(discordSocketConfig);
        Commands = new CommandService();
        DiscordSocketClient.MessageReceived += MessageReceivedAsync;
    }

    public async Task<bool> StartAsync(ServiceProvider services, string? botToken)
    {
        try
        {
            var botStillStarting = new CancellationTokenSource();
            DiscordSocketClient.Ready += botStillStarting.CancelAsync;

            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

            if (string.IsNullOrWhiteSpace(botToken))
            {
                Log.Error("Invalid token.");
                return false;
            }
            await DiscordSocketClient.LoginAsync(TokenType.Bot, botToken);
            await DiscordSocketClient.StartAsync();

            Log.Information("Bot starting...");

            while (!botStillStarting.IsCancellationRequested)
            {
                Log.Verbose("Waiting...");
                await Task.Delay(100, botStillStarting.Token);
            }

            //_ = DiscordSocketClient
            //    .GetChannelAsync(1270659363132145796)
            //    .AsTask()
            //    .ContinueWith(async t =>
            //    {
            //        if (t.Result is ITextChannel textChannel)
            //            await textChannel.SendMessageAsync("https://tenor.com/view/pillar-men-awaken-my-masters-awaken-jojos-bizarre-adventure-jjba-gif-19344086");
            //    });


            // 238725440649428992

            var user = await DiscordSocketClient.GetUserAsync(229720939078615040);
            await user.SendMessageAsync("AWAKEN MY MASTER!");
            await user.SendMessageAsync("https://tenor.com/view/pillar-men-awaken-my-masters-awaken-jojos-bizarre-adventure-jjba-gif-19344086");

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not start bot.");
            return false;
        }
    }

    public async Task<bool> StopAsync()
    {
        try
        {
            await DiscordSocketClient.LogoutAsync();
            await DiscordSocketClient.StopAsync();
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not start bot.");
            return false;
        }
    }

    private async Task MessageReceivedAsync(SocketMessage arg)
    {
        Log.Verbose("Received Message: {newLine}{message}", Environment.NewLine, arg.CleanContent);

        if (arg is not SocketUserMessage message || message.Author.IsBot)
            return;

        await new ManualCommands(message).TriggerAllAsync();

        var position = 0;
        if (Prefix != null && message.HasCharPrefix('!', ref position))
        {
            Log.Verbose("Igonred message from '{user}': '{message}'", message.Author, message.Content);
            return;
        }

        Log.Verbose("Acting on message from '{user}': '{message}'", message.Author, message.Content);
        await Commands.ExecuteAsync(
            new SocketCommandContext(DiscordSocketClient, message),
            position,
            ServiceProvider);
    }
}
