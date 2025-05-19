using System.Text.Json.Serialization;

namespace DiscordBot.Models.Discord;

public sealed class UserSelf
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("discriminator")]
    public string Discriminator { get; set; } = string.Empty;

    [JsonPropertyName("public_flags")]
    public int? PublicFlags { get; set; }

    [JsonPropertyName("flags")]
    public int? Flags { get; set; }

    [JsonPropertyName("banner")]
    public object Banner { get; set; } = string.Empty;

    [JsonPropertyName("accent_color")]
    public int? AccentColor { get; set; }

    [JsonPropertyName("global_name")]
    public string GlobalName { get; set; } = string.Empty;

    [JsonPropertyName("avatar_decoration_data")]
    public object AvatarDecorationData { get; set; } = string.Empty;

    [JsonPropertyName("collectibles")]
    public object Collectibles { get; set; } = string.Empty;

    [JsonPropertyName("banner_color")]
    public string BannerColor { get; set; } = string.Empty;

    [JsonPropertyName("clan")]
    public object Clan { get; set; } = string.Empty;

    [JsonPropertyName("primary_guild")]
    public object PrimaryGuild { get; set; } = string.Empty;

    [JsonPropertyName("mfa_enabled")]
    public bool? MfaEnabled { get; set; }

    [JsonPropertyName("locale")]
    public string Locale { get; set; } = string.Empty;

    [JsonPropertyName("premium_type")]
    public int? PremiumType { get; set; }
}