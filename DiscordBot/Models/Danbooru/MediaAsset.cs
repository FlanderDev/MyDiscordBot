using System.Text.Json.Serialization;

namespace DiscordBot.Models.Danbooru;

public class MediaAsset
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;

    [JsonPropertyName("file_ext")]
    public string FileExt { get; set; } = string.Empty;

    [JsonPropertyName("file_size")]
    public int? FileSize { get; set; }

    [JsonPropertyName("image_width")]
    public int? ImageWidth { get; set; }

    [JsonPropertyName("image_height")]
    public int? ImageHeight { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("file_key")]
    public string FileKey { get; set; } = string.Empty;

    [JsonPropertyName("is_public")]
    public bool? IsPublic { get; set; }

    [JsonPropertyName("pixel_hash")]
    public string PixelHash { get; set; } = string.Empty;

    [JsonPropertyName("variants")]
    public List<Variant> Variants { get; set; } = [];
}


