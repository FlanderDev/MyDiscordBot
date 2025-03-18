using DiscordBot.Business.Bots;
using DiscordBot.Database;
using DiscordBot.Models.Enteties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Reflection;

AppDomain.CurrentDomain.UnhandledException += (o, e) => Log.Error(e.ExceptionObject as Exception, "Unhandled Exception.");

// Ensure database has been created and has an entry.
var context = new DatabaseContext();
if (context.Database.EnsureCreated())
{
    context.AudioClips.Add(new AudioClip
    {
        DiscordUserId = 229720939078615040,
        CallCode = "xcode",
        FileName = @"E:\aras\ara-471.mp3",
    });

    context.DiscordUsers.Add(new DiscordUser
    {
        Id = 229720939078615040,
        GlobalName = "flander_lander",
        Username = "Flan"
    });

    context.SaveChanges();
}

try
{
    Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Log/log.txt", LogEventLevel.Information)
    .WriteTo.Console(LogEventLevel.Verbose) //Default: Verbose
    .CreateLogger();

    Log.Information("Initialized logging.");

    var runningSpaceNamespace = typeof(DiscordBot.RunningSpace.RunningSpace).Namespace?.Split('.').Last();
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

    var configuration = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .Build();

    await using var serviceProvider = new ServiceCollection()
        .AddLogging(a => a.SetMinimumLevel(LogLevel.Trace))
        .AddSingleton<IConfiguration>(configuration)
        .AddScoped<InaNisBot>()
        .AddScoped<KumoBot>()
        .BuildServiceProvider();


    //var testingBot = serviceProvider.GetRequiredService<TestingBot>();
    //var testing = await testingBot.StartAsync(serviceProvider);

    var testingBot = serviceProvider.GetRequiredService<InaNisBot>();
    var resultTesting = await testingBot.StartAsync(serviceProvider, configuration["DiscordBot:Token"], configuration["DiscordBot:Name"]);

    //var kumoBot = serviceProvider.GetRequiredService<KumoBot>();
    //var resultKumo = await kumoBot.StartAsync(serviceProvider, configuration["DiscordBotTesting:Token"], configuration["DiscordBotTesting:Name"]);

    Log.Verbose("ready and waiting...");
    await Task.Delay(Timeout.Infinite);
    Log.Information("Shutting down...");

    await testingBot.StopAsync();
    //await kumoBot.StopAsync();

    Log.Information("Ran to completion.");
    return 0;
}
catch (Exception ex)
{
    Log.Error(ex, "Unexpected error. Ending application.");
    return -1;
}