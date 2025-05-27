using DiscordBot.Components.Pages;
using DiscordBot.Components.Pages.Chat;
using DiscordBot.Components.Pages.Clip;
using DiscordBot.Components.Pages.User;
using Microsoft.AspNetCore.Components;
using Serilog;
using System.Reflection;

namespace DiscordBot.Business.Helpers.Blazor;

public static class RouteHelper
{
    internal static readonly string Home = GetPageRouteTemplate<Home>();
    internal static readonly string Error = GetPageRouteTemplate<Error>();
    internal static readonly string Clip = GetPageRouteTemplate<Clip>();
    internal static readonly string Chat = GetPageRouteTemplate<Chat>();
    internal static readonly string Login = GetPageRouteTemplate<Login>();

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
}
