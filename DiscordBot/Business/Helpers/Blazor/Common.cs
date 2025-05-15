using System.Reflection;
using Microsoft.AspNetCore.Components;
using Serilog;

namespace DiscordBot.Business.Helpers.Blazor;

public static class Common
{
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
