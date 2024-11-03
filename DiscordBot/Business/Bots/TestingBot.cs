using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Definitions;
using Serilog;

namespace DiscordBot.Business.Bots;
public sealed class TestingBot : IBot
{
    private ServiceProvider? _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly string _name;

    public TestingBot(IConfiguration configuration)
    {
        _configuration = configuration;
        _name = configuration["DiscordBot:Name"] ?? "{missingDiscordName}";
        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };

        _client = new DiscordSocketClient(discordSocketConfig);
        _commands = new CommandService();
    }

    public async Task StartAsync(ServiceProvider services)
    {
        _serviceProvider = services;
        await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

        var botToken = _configuration["DiscordBotTesting:Token"];
        if (string.IsNullOrWhiteSpace(botToken))
        {
            Log.Error("{name}: Invalid token.", _name);
            return;
        }

        await _client.LoginAsync(TokenType.Bot, botToken);
        await _client.StartAsync();

        _client.MessageReceived += MessageReceived;
        _client.UserVoiceStateUpdated += UserVoiceStateUpdated;
    }

    public async Task StopAsync()
    {
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    private async Task UserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
    {
        if (arg1.IsBot)
            return;

        if (arg1 is not IGuildUser guildUser)
        {
            Log.Warning("{name}: User : '{user}' with Id '{id}' is not '{interface}'.", _name, arg1.GlobalName, arg1.Id, nameof(IGuildUser));
            return;
        }

        //Log.Verbose("{name}: User: '{user}' voice state changed from '{fromArg}' to '{toArg}'.", _name, arg1.GlobalName, arg2.);
    }

    private async Task MessageReceived(SocketMessage arg)
    {
        // Ignore messages from bots
        if (arg is not SocketUserMessage message || message.Author.IsBot)
            return;

        // Check if the message starts with !
        var position = 0;
        message.HasCharPrefix('!', ref position);

        // Execute the command if it exists in the ServiceCollection
        await _commands.ExecuteAsync(
            new SocketCommandContext(_client, message),
            position,
            _serviceProvider);
    }
}
