using DiscordBot.Components.Pages;
using DiscordBot.Components.Pages.Clip;
using DiscordBot.Components.Pages.Clip.Partial;
using DiscordBot.Components.Pages.User;
using Microsoft.AspNetCore.Components;
using Serilog;
using System.Reflection;
using DiscordBot.Components.Pages.Control;

namespace DiscordBot.Business.Helpers.Blazor;

public static class RouteHelper
{
    internal static readonly string Home = GetPageRouteTemplate<Home>();
    internal static readonly string Error = GetPageRouteTemplate<Error>();
    internal static readonly string Login = GetPageRouteTemplate<Login>();
    internal static readonly string Logout = GetPageRouteTemplate<Logout>();

    internal static readonly string ControlChat = GetPageRouteTemplate<Chat>();
    internal static readonly string ControlVoice = GetPageRouteTemplate<VoiceControl>();

    internal static readonly string Clip = GetPageRouteTemplate<Clip>();
    internal static readonly string YouTubeEditor = GetPageRouteTemplate<YouTubeEditor>();

    internal static IReadOnlyCollection<string> PageRoutes = GetDefinedRoutes();

    public static string GetPageRouteTemplate<T>() where T : ComponentBase
    {
        var type = typeof(T);
        var routeAttribute = type.GetCustomAttributes<RouteAttribute>().FirstOrDefault();
        if (routeAttribute != null)
        {
            Log.Verbose("Route template of '{component}' is '{template}'.", type.FullName, routeAttribute.Template);
            return routeAttribute.Template;
        }

        Log.Error("The component '{component}' does not have a '{route}'.", type.FullName, nameof(RouteAttribute));
        return $"/Error?={type.FullName}";
    }

    private static List<string> GetDefinedRoutes()
    {
        var componentBase = typeof(ComponentBase);
        var componentRoutes = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(w => w.BaseType == componentBase)
            .Select(s => s.GetCustomAttributes<RouteAttribute>().FirstOrDefault(f => !f.Template .Contains('{'))?.Template ?? string.Empty)
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToList();

        return componentRoutes;
    }
}
