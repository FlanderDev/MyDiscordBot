using System.Diagnostics;
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
    var errorCode = SetRunningDirectory();
    if (errorCode != null)
        return errorCode.Value;

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.File("Log/log.txt", restrictedToMinimumLevel: LogEventLevel.Information)
        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose) //Default: Verbose
        .CreateLogger();
    Log.Information("Initialized logging.");

    AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Error(e.ExceptionObject as Exception, "Unhandled Exception.");

    Console.WriteLine("---------TEST-------------");
    var files = Directory.GetFiles(Environment.SystemDirectory, "*", SearchOption.AllDirectories);
    foreach (var file in files)
        Console.WriteLine(file);

    var result = Process.Start("ffmpeg", "-h").StandardOutput.ReadToEnd();
    Console.WriteLine(string.IsNullOrWhiteSpace(result) ? "FUCK" : result);
    Console.WriteLine("---------DONE-------------");

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

    if (configuration.GetLogValue("Dependencies:Disable") == null)
        await DependencyHelper.LoadMissingAsync();

    DatabaseContext.CreateDefault();

    if (configuration.GetLogValue("Danbooru:Token") is not { } danbooruToken)
        Log.Warning("No danbooru token has been provided, some functionality won't work.");
    else
        DanbooruHelper.ApiKey = danbooruToken;

    var testingBot = serviceProvider.GetRequiredService<InaNisBot>();
    await testingBot.StartAsync(serviceProvider, botTokenValue);

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
    var runningSpace = Path.Combine(Environment.CurrentDirectory, "RunningSpace");
    if (!Directory.CreateDirectory(runningSpace).Exists)
    {
        Log.Fatal("Invalid running space: '{invalidRunningSpace}'.", runningSpace);
        return -2;
    }

    Environment.CurrentDirectory = runningSpace;
    Log.Information("Running in '{currentDirectory}'.", Environment.CurrentDirectory);
    return null;
}