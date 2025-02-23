using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Reflection;

namespace DiscordBot.Business.Bots;

internal abstract class BotBase
{
    internal DiscordSocketClient DiscordSocketClient { get; }
    internal CommandService Commands { get; }
    internal IServiceProvider ServiceProvider { get; private set; }
    internal string? Name { get; private set; }
    internal char? Prefix { get; private set; }

    public BotBase(IServiceProvider serviceProvider, char? prefix = null)
    {
        ServiceProvider = serviceProvider;
        Prefix = prefix;

        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        DiscordSocketClient = new DiscordSocketClient(discordSocketConfig);
        Commands = new CommandService();
        DiscordSocketClient.MessageReceived += MessageReceived;
    }

    public async Task<bool> StartAsync(ServiceProvider services, string? botToken, string? name)
    {
        try
        {
            Name = name;
            await Commands.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

            if (string.IsNullOrWhiteSpace(botToken))
            {
                Log.Error("{name}: Invalid token.", Name);
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                Log.Error("{name}: Invalid name.", Name);
                return false;
            }

            await DiscordSocketClient.LoginAsync(TokenType.Bot, botToken);
            await DiscordSocketClient.StartAsync();

            Log.Information("{name}: Started.", Name);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "{name}: Could not start.", Name);
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
            Log.Error(ex, "{name}: Could not stop.", Name);
            return false;
        }
    }

    private async Task MessageReceived(SocketMessage arg)
    {
        Log.Verbose(arg.Content);
        if (arg is not SocketUserMessage message || message.Author.IsBot)
            return;

        var position = 0;
        if (Prefix != null && message.HasCharPrefix('!', ref position))
        {
            Log.Verbose("{name}: Igonred message from '{user}': '{message}'", Name, message.Author, message.Content);
            return;
        }

        Log.Verbose("{name}: Acting on message from '{user}': '{message}'", Name, message.Author, message.Content);
        await Commands.ExecuteAsync(
            new SocketCommandContext(DiscordSocketClient, message),
            position,
            ServiceProvider);
    }
}
