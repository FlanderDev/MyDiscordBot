using DiscordBot.Components.Pages.Clip;
using Microsoft.AspNetCore.Components;
using Serilog;
using System.Reflection;
using DiscordBot.Components.Pages;

namespace DiscordBot.Business.Helpers.Blazor;

public static class RouteHelper
{
    internal static readonly string Home = GetRelativeRoute<Home>() ?? "/Error";
    internal static readonly string Error = GetRelativeRoute<Error>() ?? "/Error";
    internal static readonly string ClipManaging = GetRelativeRoute<Manage>() ?? "/Error";
    internal static readonly string ClipUpload = GetRelativeRoute<Upload>() ?? "/Error";

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
