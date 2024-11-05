using System.Text.Json.Serialization;

namespace DiscordBot.Models.Danbooru;

public class Variant
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("file_ext")]
    public string FileExt { get; set; } = string.Empty;
}


