using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Business.Commands;
using DiscordBot.Models.Internal.Configs;
using Microsoft.Extensions.Options;
using Serilog;
using System.Diagnostics;
using System.Reflection;

namespace DiscordBot.Business.Bots;

public sealed class DiscordNet(IOptions<Configuration> options, IServiceProvider serviceProvider) : IHostedService
{
    internal ITextChannel? DebugChannel;

    internal DiscordSocketClient DiscordSocketClient = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
    });

    private readonly CommandService _commands = new();

    private static string[] LoginMessages => [
        $"It's {DateTime.Now:T} and I'm ready to fuck shit up!",
        "> You know the pepeloni? The nooo one.",
        "'Have confidence!' *Dies* 'No confidence!'",
        "> Watame... warukunai yo ne",
        "> Hi Honey~",
        "> Glasses are very versatile..",
        "> Hot dog is the enemy.",
        "> I'm Horny!",
        "> BEEF PC!",
        "> Polutato Pishi? Senk yu, senk yu",
        "> Kuso.....KuSOOO.... ",
        "Nothing beats a G R O U N D P O U N D ! *bass drops*",
        "> It's legal to pop bubblegum, but it's not legal to pop the cherry! ",
        "https://www.youtube.com/watch?v=YURh1p7oKEA&t=4985s",
        "# OMAEEE",
        "> you can't be mad at me. i'm cute ",
        "# YA BE",
        "> WAH!",
        "> hello overseas sexys guys.",
        "> I'm God, okay?",
        "> Oh I'm die thank you forever",
        "> Violence is the solution to everything.",
        "> Things in this world are too weak.",
        "> This too, is Hololive",
        "> Shut up, b*TCH, HA\u2197\ufe0fHA\u2198\ufe0fHA\u2197\ufe0fHA\u2198\ufe0f",
        "> Glasses are really versatile. First, you can have glasses-wearing girls take them off and suddenly become beautiful, or have girls wearing glasses flashing those cute grins, or have girls stealing the protagonist's glasses and putting them on like, \"Haha, got your glasses!' That's just way too cute! Also, boys with glasses! I really like when their glasses have that suspicious looking gleam, and it's amazing how it can look really cool or just be a joke. I really like how it can fulfill all those abstract needs. Being able to switch up the styles and colors of glasses based on your mood is a lot of fun too! It's actually so much fun! You have those half rim glasses, or the thick frame glasses, everything! It's like you're enjoying all these kinds of glasses at a buffet. I really want Luna to try some on or Marine to try some on to replace her eyepatch. We really need glasses to become a thing in hololive and start selling them for HoloComi. Don't. You. Think. We. Really. Need. To. Officially. Give. Everyone. Glasses? -Fubuki ",
        "> Life is 9% Alcohol",
    ];

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
            DiscordSocketClient.Disconnected += async _ => await StopAsync(cancellationToken);
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
            await DiscordSocketClient.LoginAsync(TokenType.Bot, options.Value.Discord.Token);
            await DiscordSocketClient.StartAsync();


            Log.Information("Bot starting...");
            var stopwatch = Stopwatch.StartNew();
            await botBooting.Task.WaitAsync(cancellationToken);
            stopwatch.Stop();
            Log.Information("Bot started. It took {time}", stopwatch.Elapsed.ToString("c"));

#if !DEBUG
            if (await DiscordSocketClient.GetChannelAsync(1270659363132145796) is ITextChannel textChannel)
            {
                DebugChannel = textChannel;
                var messages = LoginMessages.Length;
                var message = LoginMessages[Random.Shared.Next(messages - 1)];
                await textChannel.SendMessageAsync(message);
            }
#endif
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
            Log.Information("Stopped {name}.", nameof(DiscordNet));
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
                message.Content, serviceProvider);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reacting to {name}.", nameof(MessageReceivedAsync));
        }
    }
}
