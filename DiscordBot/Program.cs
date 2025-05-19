using DiscordBot.Business.Bots;
using DiscordBot.Business.Services;
using DiscordBot.Components;
using DiscordBot.Data;
using DiscordBot.Models.Internal.Configs;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using System.Reflection;

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

    // this kinda of validation is... special :d
    builder.Services.Configure<Configuration>(builder.Configuration)
        .AddOptionsWithValidateOnStart<Configuration>()
        .Validate(RecursivelyNotNullOrWhiteSpace)
        .ValidateOnStart();

    if (!DatabaseContext.CreateDefault())
    {
        Log.Warning("Failed to create default database, stopping application.");
        return 101;
    }

    builder.Services
        .AddSerilog()
        .AddHttpContextAccessor()
        .AddHealthChecks();

    builder.Services
        .AddScoped<DanbooruService>()
        .AddHostedService<DiscordNet>();
    //.AddSingleton(f => new DiscordNet(f.GetRequiredService<IOptions<Configuration>>(), f));

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

    app.MapHealthChecks("/status");

    Log.Verbose("Running web application.");

    app.Run();

    Log.Information("Application ran to completion.");
    return 0;
}
catch (OptionsValidationException ex) // The var name 'IoEx' would cause confusion for sure here, no? :D
{
    Log.Fatal(ex, "The configuration is missing some value. Double check your key spellings.");
    return 2;
}
catch (Exception ex)
{
    Log.Fatal(ex, ex.InnerException == null ? "Unexpected fatal error." : "Unexpected fatal error. See inner exception.");
    Log.Fatal(ex.InnerException, "Inner exception of crash reason.");
    return 1;
}
finally
{
    Log.Information("Ending application.");
    Log.CloseAndFlush();
}

static bool RecursivelyNotNullOrWhiteSpace(object instance)
{
    foreach (var property in instance.GetType().GetProperties())
    {
        var propertyValue = property.GetValue(instance);
        switch (propertyValue)
        {
            case null:
                return false;
            case string text when !string.IsNullOrWhiteSpace(text):
                continue;
        }

        // Probably should check for more but classes, but meh
        if (property.PropertyType.IsClass &&
            !property.PropertyType.Name.Equals("String", StringComparison.OrdinalIgnoreCase) &&
            !RecursivelyNotNullOrWhiteSpace(propertyValue))
            return false;
    }
    return true;
}
