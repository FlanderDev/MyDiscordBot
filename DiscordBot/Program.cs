using DiscordBot.Business.Bots;
using DiscordBot.Business.Helpers;
using DiscordBot.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Reflection;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.File("Log/log.txt", restrictedToMinimumLevel: LogEventLevel.Information)
        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose) //Default: Verbose
        .CreateLogger();
    Log.Information("Initialized logging.");

    AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Error(e.ExceptionObject as Exception, "Unhandled Exception.");

    var configuration = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
        .AddEnvironmentVariables()
        .Build();

    await using var serviceProvider = new ServiceCollection()
        .AddLogging(a => a.SetMinimumLevel(LogLevel.Trace))
        .AddSingleton<IConfiguration>(configuration)
        .AddScoped<InaNisBot>()
        .BuildServiceProvider();

    if (configuration.GetLogValue("DiscordBot:Token") is not { } botTokenValue)
    {
        Log.Warning("No discord token has been provided, stopping application.");
        return 100;
    }

    DatabaseContext.CreateDefault();

    if (configuration.GetLogValue("Danbooru:Token") is not { } danbooruToken)
        Log.Warning("No danbooru token has been provided, some functionality won't work.");
    else
        DanbooruHelper.ApiKey = danbooruToken;

    var testingBot = serviceProvider.GetRequiredService<InaNisBot>();
    await testingBot.StartAsync(serviceProvider, botTokenValue);

    AppDomain.CurrentDomain.ProcessExit += async (o, e) =>
    {
        Log.Verbose($"Shutdown received! Debug channel available: {testingBot.DebugChannel != null}");
        await (testingBot.DebugChannel?.SendMessageAsync("Sorry folks, I'm heading out^^") ?? Task.CompletedTask);
        await testingBot.StopAsync();
        Log.Verbose("literally just waiting and stretching it out.");
        await Task.Delay(30000);
        Log.Verbose("Okay, exiting for real now.");
    };

    await Task.Delay(5000);
    Environment.Exit(0);

    Log.Verbose("ready and waiting...");
    await Task.Delay(Timeout.Infinite);

    Log.Warning("Shutting down by passing infinity. Yes really!");
    await testingBot.StopAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Error(ex, "Unexpected error. Ending application.");
    return -1;
}
finally
{
    Log.Verbose("Finally, bye");
    Log.CloseAndFlush();
}
