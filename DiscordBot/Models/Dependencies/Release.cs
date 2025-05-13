using System.Text.Json.Serialization;

namespace DiscordBot.Models.Dependencies;

public sealed class Release
{
    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; } = [];
}
