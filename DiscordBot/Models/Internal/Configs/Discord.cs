using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace DiscordBot.Models.Internal.Configs;

public sealed class Discord
{
    public string Token { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public IReadOnlyCollection<ulong> IdOfAdmins { get; set; } = [];
    public int ReconnectAttemptsMax { get; set; }
    public int ReconnectAttemptDelaySeconds { get; set; }
}
