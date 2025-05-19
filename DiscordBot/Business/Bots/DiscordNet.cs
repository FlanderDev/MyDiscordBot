using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Business.Commands;
using DiscordBot.Models.Internal.Configs;
using Microsoft.Extensions.Options;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using DiscordBot.Business.Services;

namespace DiscordBot.Business.Bots;

public sealed class DiscordNet(IOptions<Configuration> options, IServiceProvider serviceProvider) : IHostedService
{
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
            Log.Verbose("Starting {name} service.", nameof(DiscordNet));

            if (string.IsNullOrWhiteSpace(options.Value.Discord.Token))
            {
                Log.Error("Invalid token.");
                throw new ArgumentException("The token is invalid.");
            }

            var botBooting = new TaskCompletionSource();
            DiscordSocketClient.Ready += () => Task.Run(botBooting.SetResult, cancellationToken);

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
            await DiscordSocketClient.LoginAsync(TokenType.Bot, options.Value.Discord.Token);
            await DiscordSocketClient.StartAsync();


            Log.Information("Bot starting...");
            var stopwatch = Stopwatch.StartNew();
            await botBooting.Task.WaitAsync(cancellationToken);
            stopwatch.Stop();
            Log.Information("Bot started. It took {time}", stopwatch.Elapsed.ToString("c"));

            if (await DiscordSocketClient.GetChannelAsync(1270659363132145796) is ITextChannel textChannel)
            {
                DebugChannel = textChannel;
                await textChannel.SendMessageAsync($"It's {DateTime.Now:T} and I'm ready to fuck shit up!");
            }

            DiscordSocketClient.MessageReceived += MessageReceivedAsync;

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
            Log.Information("Stopping {name}...", nameof(DiscordNet));
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
        try
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
        catch (Exception ex)
        {
            Log.Error(ex, "Error reacting to {name}.", nameof(MessageReceivedAsync));
        }
    }
}
