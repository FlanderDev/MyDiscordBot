using System.Reflection;
using DiscordBot.Business.Bots;
using DiscordBot.Definitions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace DiscordBot;

internal static class Program
{
    private static async Task<int> Main()
    {
        try
        {
            await using var log = new LoggerConfiguration()
            .WriteTo.Console() //Default: Verbose
            .WriteTo.File("Log/log.txt", LogEventLevel.Information)
            .CreateLogger();

            log.Information("Initialized logging.");

            var runningSpaceNamespace = typeof(RunningSpace.RunningSpace).Namespace;
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
                .AddSingleton<IConfiguration>(configuration)
                .AddScoped<IBot, TestingBot>()
                .BuildServiceProvider();

            var bot = serviceProvider.GetRequiredService<IBot>();
            await bot.StartAsync(serviceProvider);

            do
            {
                Console.WriteLine("Press 'q' to quit.");
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
            Log.Information("Shutting down...");

            await bot.StopAsync();

            Log.Information("Ran to completion.");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error. Ending application.");
            return -1;
        }
    }
}