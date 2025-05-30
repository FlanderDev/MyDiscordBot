using DiscordBot.Business.Bots;
using DiscordBot.Business.Helpers.Blazor;
using DiscordBot.Business.Helpers.Bot;
using DiscordBot.Components;
using DiscordBot.Data;
using DiscordBot.Models.Internal.Configs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using System.Reflection;

try
{
#if DEBUG
    Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory; // Since files will be moved into the running directory and I don't want them in my project dir.
#endif

    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.File("Log/log.txt", restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
    
    .CreateLogger();

    Log.Information("Initialized logging.");

    AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Fatal(e.ExceptionObject as Exception, "Unhandled Exception.");


    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(Log.Logger);
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger);

    builder.Configuration
        .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
        .AddEnvironmentVariables();

    // this kind of validation is... special :d
    builder.Services.Configure<Configuration>(builder.Configuration)
        .AddOptionsWithValidateOnStart<Configuration>()
        .Validate(StartupHelper.RecursivelyNotNullOrWhiteSpace)
        .ValidateOnStart();

    builder.Services
        .AddSerilog()
        .AddHttpContextAccessor()
        .AddHealthChecks();

    builder.Services.AddScoped<LoginService>();

    FileHelper.AppStorage.Create();
    if (!await DatabaseContext.CreateDefaultAsync())
    {
        Log.Warning("Failed to create default database, stopping application.");
        return 101;
    }

    if (!StartupHelper.MoveRequiredFiles())
    {
        Log.Fatal("Failed to ensure required files, stopping application.");
        return 102;
    }

    // TODO: Dependency Injection resolve problem.
    // I want to use DI so services can be used in the DiscordCommands.
    // However, doing right now results in an error, because the commands get their scope by the registered DiscordNet instance,
    // and if a command then requires a service, it can't be resolved by the following code in DiscordNet:
    // await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);
    // TL;DR: Problem for future me.
    builder.Services
        .AddDbContext<DatabaseContext>()
        // Different problem: 'AddHostedService' doesn't register it for DI, so I'm adding a singleton and requesting it.
        .AddSingleton<DiscordNet>()
        .AddHostedService(o => o.GetRequiredService<DiscordNet>());

    builder.Services
        .AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
        {
            o.LoginPath = RouteHelper.Login;
            o.LogoutPath = RouteHelper.Logout;
            o.AccessDeniedPath = RouteHelper.Error;
        });

    builder.Services
        .AddAuthorization()
        .AddControllers();

    var app = builder.Build();
    if (!app.Environment.IsDevelopment())
        app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers();
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

