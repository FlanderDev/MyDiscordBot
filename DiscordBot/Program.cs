using DiscordBot.Business.Bots;
using DiscordBot.Components;
using DiscordBot.Data;
using Serilog;
using Serilog.Events;
using System.Reflection;
using DiscordBot.Models.Internal;
using Microsoft.Extensions.Options;

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

    builder.Configuration
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
        .AddEnvironmentVariables();

    // 3 extra lines, just to validate options... I hate it.
    builder.Services.Configure<Configuration>(builder.Configuration)
        .AddOptionsWithValidateOnStart<Configuration>()
        .ValidateDataAnnotations()
        .ValidateOnStart();

    if (!DatabaseContext.CreateDefault())
    {
        Log.Warning("Failed to create default database, stopping application.");
        return 101;
    }

    //_ = discordNet.StartAsync(default); // Not waiting for this, since we don't work with the result & if it crashes the application ends.

    builder.Services
        .AddSerilog()
        .AddHostedService<DiscordNet>();

    builder.Services
        .AddRazorComponents()
        .AddInteractiveServerComponents();

    var app = builder.Build();
    if (!app.Environment.IsDevelopment())
        app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseAntiforgery();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Verbose("Running web application.");
    app.Run();
    return 0;
}
catch (OptionsValidationException ex) // The var name 'IoEx' would cause confusion for sure here, no? :D
{
    Log.Fatal(ex, "The configuration is missing some value. Double check your key spellings.");
    return 2;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unexpected fatal error.");
    return 1;
}
finally
{
    Log.Information("Ending application.");
    Log.CloseAndFlush();
}
