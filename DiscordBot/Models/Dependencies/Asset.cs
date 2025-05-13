using System.Text.Json.Serialization;

namespace DiscordBot.Models.Dependencies;

public sealed class Asset
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;
}
