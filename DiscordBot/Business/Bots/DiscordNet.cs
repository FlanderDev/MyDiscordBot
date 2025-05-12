using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Business.Commands;
using Serilog;
using System.Diagnostics;
using System.Reflection;

namespace DiscordBot.Business.Bots;

public sealed class DiscordNet : IHostedService
{
    internal string? Token { get; set; }
    internal ITextChannel? DebugChannel;
    internal DiscordSocketClient DiscordSocketClient = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
    });

    private readonly CommandService _commands = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                Log.Error("Invalid token.");
                throw new ArgumentException("The token is invalid.");
            }

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), null);

            var botBooting = new TaskCompletionSource();
            DiscordSocketClient.Ready += () => Task.Run(botBooting.SetResult, cancellationToken);

            await DiscordSocketClient.LoginAsync(TokenType.Bot, Token);
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

            DiscordSocketClient.MessageReceived += MessageReceivedAsync;
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                Log.Verbose($"Shutdown received! Debug channel available: {DebugChannel != null}");
                // awaiting the task HERE would signal the runtime, that there is nothing to do on this thread, thus allowing continuation of the AppDomain shutdown.
                DebugChannel?.SendMessageAsync("Sorry folks, I'm heading out^^").GetAwaiter().GetResult();
                StopAsync(default).GetAwaiter().GetResult();
                Log.Verbose("Done shutting down.");
            };

            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException("Canceled bot initialization.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not start bot.");
            throw new Exception("The bot failed to initialize.", ex);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await DiscordSocketClient.LogoutAsync();
            await DiscordSocketClient.StopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not stop the bot.");
            throw new Exception("Error stopping bot.", ex);
        }
    }

    private async Task MessageReceivedAsync(SocketMessage arg)
    {
        Log.Verbose("Received Message: {newLine}{message}", Environment.NewLine, arg.CleanContent);

        if (arg is not SocketUserMessage message || message.Author.IsBot)
            return;

        await new ManualCommands(message).TriggerAllAsync();

        Log.Verbose("Received message from '{user}': '{message}'", message.Author, message.Content);
        await _commands.ExecuteAsync(
            new SocketCommandContext(DiscordSocketClient, message),
            message.Content, null);
    }
}
