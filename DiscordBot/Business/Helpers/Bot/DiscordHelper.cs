using Discord;
using Discord.Commands;
using System.Security.Claims;

namespace DiscordBot.Business.Helpers.Bot;

internal static class DiscordHelper
{
    internal static IVoiceChannel? GetVoiceChannel(this ModuleBase<SocketCommandContext> moduleBase) => (moduleBase.Context.User as IGuildUser)?.VoiceChannel;

    internal static (ulong DiscordId, string GlobalName)? GetUserFromHttpContextAccessor(this IHttpContextAccessor httpContextAccessor)
    {
        var sidClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(f => f.Type.Equals(ClaimTypes.Sid));
        if (sidClaim == null)
            return null;

        var discordId = ulong.TryParse(sidClaim.Value, out var idText) ? idText : (ulong?)null;
        if (discordId == null)
            return null;

        var nameClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(f => f.Type.Equals(ClaimTypes.Name));
        if (nameClaim == null)
            return null;
        return (discordId.Value, nameClaim.Value);
    }
}
