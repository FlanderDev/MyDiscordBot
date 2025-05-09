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

    if (!DatabaseContext.CreateDefault())
    {
        Log.Warning("Failed to create default database, stopping application.");
        return 101;
    }

    if (configuration.GetLogValue("Danbooru:Token") is not { } danbooruToken)
        Log.Warning("No danbooru token has been provided, some functionality won't work.");
    else
        DanbooruHelper.ApiKey = danbooruToken;

    var testingBot = serviceProvider.GetRequiredService<InaNisBot>();
    await testingBot.StartAsync(serviceProvider, botTokenValue);

    AppDomain.CurrentDomain.ProcessExit += (_, _) =>
    {
        Log.Verbose($"Shutdown received! Debug channel available: {testingBot.DebugChannel != null}");
        // awaiting the task HERE would signal the runtime, that there is nothing to do on this thread, thus allowing continuation of the AppDomain shutdown.
        testingBot.DebugChannel?.SendMessageAsync("Sorry folks, I'm heading out^^").GetAwaiter().GetResult();
        testingBot.StopAsync().GetAwaiter().GetResult();
        Log.Verbose("Done shutting down.");
    };

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
