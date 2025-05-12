using System.Reflection;
using DiscordBot.Business.Helpers;
using DiscordBot.Components;
using DiscordBot.Data;
using Serilog.Events;
using Serilog;
using DiscordBot.Business.Bots;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.File("Log/log.txt", restrictedToMinimumLevel: LogEventLevel.Information)
        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose)
        .CreateLogger();

    Log.Information("Initialized logging.");

    AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Error(e.ExceptionObject as Exception, "Unhandled Exception.");


    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(Log.Logger);
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger);

    var configuration = builder.Configuration
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
        .AddEnvironmentVariables()
        .Build();

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

    var discordNet = new DiscordNet
    {
        Token = botTokenValue
    };

    builder.Services
        .AddSerilog()
        .AddSingleton(discordNet)
        .AddRazorComponents()
        .AddInteractiveServerComponents();

    var app = builder.Build();
    if (!app.Environment.IsDevelopment())
        app.UseExceptionHandler("/Error", createScopeForErrors: true);


    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    await discordNet.StartAsync(default);
    Log.Verbose("Ready...");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Error(ex, "Unexpected error. Ending application.");
    return 1;
}
finally
{
    Log.Verbose("Finally, bye");
    Log.CloseAndFlush();
}



