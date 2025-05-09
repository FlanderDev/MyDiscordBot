using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Business.Commands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Diagnostics;
using System.Reflection;

namespace DiscordBot.Business.Bots;

public sealed class InaNisBot
{
    internal DiscordSocketClient DiscordSocketClient { get; }
    internal CommandService Commands { get; }
    internal IServiceProvider ServiceProvider { get; private set; }
    internal char? Prefix { get; private set; }

    internal ITextChannel? DebugChannel;

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
            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

            if (string.IsNullOrWhiteSpace(botToken))
            {
                Log.Error("Invalid token.");
                return false;
            }

            var botBooting = new TaskCompletionSource();
            DiscordSocketClient.Ready += () => Task.Run(botBooting.SetResult);
            await DiscordSocketClient.LoginAsync(TokenType.Bot, botToken);
            await DiscordSocketClient.StartAsync();

            Log.Information("Bot starting...");
            var stopwatch = Stopwatch.StartNew();
            await botBooting.Task;

            stopwatch.Stop();
            Log.Information("Bot started. It took {time}", stopwatch.Elapsed.ToString("c"));

            if (await DiscordSocketClient.GetChannelAsync(1270659363132145796) is ITextChannel textChannel)
            {
                DebugChannel = textChannel;
#if !DEBUG
                await textChannel.SendMessageAsync( $"It's {DateTime.Now:T} and I'm ready to fuck shit up!");
#endif
            }

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
            Log.Error(ex, "Could not stop the bot.");
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
            Log.Verbose("Ignored message from '{user}': '{message}'", message.Author, message.Content);
            return;
        }

        Log.Verbose("Received message from '{user}': '{message}'", message.Author, message.Content);
        await Commands.ExecuteAsync(
            new SocketCommandContext(DiscordSocketClient, message),
            position,
            ServiceProvider);
    }
}
