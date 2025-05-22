using Microsoft.AspNetCore.Components;
using Serilog;
using System.Reflection;
using DiscordBot.Components.Pages;
using System.Text;
using DiscordBot.Components.Pages.Chat;
using DiscordBot.Components.Pages.Clip;

namespace DiscordBot.Business.Helpers.Blazor;

public static class RouteHelper
{
    internal static readonly string Home = GetRelativeRoute<Home>() ?? "/Error";
    internal static readonly string Error = GetRelativeRoute<Error>() ?? "/Error";
    internal static readonly string Clip = GetRelativeRoute<Clip>() ?? "/Error";
    internal static readonly string Chat = GetRelativeRoute<Chat>() ?? "/Error";

    public static string? GetRelativeRoute<T>() where T : ComponentBase
    {
        var type = typeof(T);
        var routeAttribute = type.GetCustomAttribute<RouteAttribute>();
        if (routeAttribute != null)
            return routeAttribute.Template;

        Log.Error("The component '{component}' does not have a '{route}'.", type.FullName, nameof(RouteAttribute));
        return null;
    }
}
