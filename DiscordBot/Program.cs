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
    AppDomain.CurrentDomain.UnhandledException += (o, e) => Log.Error(e.ExceptionObject as Exception, "Unhandled Exception.");


    var errorCode = SetRunningDirectory();
    if (errorCode != null)
        return errorCode.Value;


    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.File("Log/log.txt", restrictedToMinimumLevel: LogEventLevel.Information)
        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose) //Default: Verbose
        .CreateLogger();
    Log.Information("Initialized logging.");


    DatabaseContext.CreateDefault();


    var configuration = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
        .AddEnvironmentVariables()
        .Build();

    await using var serviceProvider = new ServiceCollection()
        .AddLogging(a => a.SetMinimumLevel(LogLevel.Trace))
        .AddSingleton<IConfiguration>(configuration)
        .AddScoped<InaNisBot>()
        .BuildServiceProvider();


    var nameValue = configuration.GetLogValue("DiscordBot:Name");
    var tokenValue = configuration.GetLogValue("DiscordBot:Token");

    var testingBot = serviceProvider.GetRequiredService<InaNisBot>();
    var resultTesting = await testingBot.StartAsync(serviceProvider, tokenValue, nameValue);


    Log.Verbose("ready and waiting...");
    await Task.Delay(Timeout.Infinite);

    Log.Information("Shutting down...");
    await testingBot.StopAsync();

    Log.Information("Ran to completion.");
    return 0;
}
catch (Exception ex)
{
    Log.Error(ex, "Unexpected error. Ending application.");
    return -1;
}
finally
{
    Log.CloseAndFlush();
}

static int? SetRunningDirectory()
{
    var runningSpaceNamespace = typeof(DiscordBot.RunningSpace.RunningSpace).Namespace?.Split('.').LastOrDefault();
    if (string.IsNullOrWhiteSpace(runningSpaceNamespace))
    {
        Log.Warning("Invalid namespace.");
        return -3;
    }

    var runningSpace = Path.Combine(Environment.CurrentDirectory, runningSpaceNamespace);
    if (!Directory.Exists(runningSpace))
    {
        Log.Fatal("Invalid running space: '{invalidRunningSpace}'.", runningSpace);
        return -2;
    }

    Environment.CurrentDirectory = runningSpace;
    Log.Information($"Running in '{Environment.CurrentDirectory}'.");
    return null;
}
