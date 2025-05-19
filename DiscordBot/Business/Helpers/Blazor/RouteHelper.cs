using DiscordBot.Components.Pages.Clip;
using Microsoft.AspNetCore.Components;
using Serilog;
using System.Reflection;
using DiscordBot.Components.Pages;
using System.Text;

namespace DiscordBot.Business.Helpers.Blazor;

public static class RouteHelper
{
    internal static readonly string Home = GetRelativeRoute<Home>() ?? "/Error";
    internal static readonly string Error = GetRelativeRoute<Error>() ?? "/Error";
    internal static readonly string ClipManaging = GetRelativeRoute<Manage>() ?? "/Error";
    internal static readonly string ClipUpload = GetRelativeRoute<Upload>() ?? "/Error";

    internal static string CreateAuthUrl(Models.Internal.Configs.Discord config) =>
        new StringBuilder("https://discord.com/api/oauth2/authorize?client_id=")
            .Append(config.ClientId)
            .Append("&redirect_uri=")
            .Append(Uri.EscapeDataString(config.RedirectUri))
            .Append("&response_type=code&scope=")
            .Append(Uri.EscapeDataString(config.Scopes))
            .ToString();

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
