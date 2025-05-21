using Discord;
using Discord.Commands;
using DiscordBot.Models.Entities;
using Serilog;
using System.Security.Claims;

namespace DiscordBot.Business.Helpers.Bot;

internal static class DiscordHelper
{
    internal static IVoiceChannel? GetVoiceChannel(this ModuleBase<SocketCommandContext> moduleBase) => (moduleBase.Context.User as IGuildUser)?.VoiceChannel;

    internal static ulong? GetUserIdFromHttpContextAccessor(this IHttpContextAccessor httpContextAccessor)
    {
        var discordClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(f => f.Type.Equals(ClaimTypes.Sid));
        if (discordClaim == null)
            return null;

        return ulong.TryParse(discordClaim.Value, out var discordId) ? discordId : null;
    }
}
