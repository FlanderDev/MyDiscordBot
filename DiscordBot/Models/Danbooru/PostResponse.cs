using System.Text.Json.Serialization;

namespace DiscordBot.Models.Danbooru;

public class PostResponse
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("uploader_id")]
    public int? UploaderId { get; set; }

    [JsonPropertyName("score")]
    public int? Score { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("md5")]
    public string Md5 { get; set; } = string.Empty;

    [JsonPropertyName("last_comment_bumped_at")]
    public DateTime? LastCommentBumpedAt { get; set; }

    [JsonPropertyName("rating")]
    public string Rating { get; set; } = string.Empty;

    [JsonPropertyName("image_width")]
    public int? ImageWidth { get; set; }

    [JsonPropertyName("image_height")]
    public int? ImageHeight { get; set; }

    [JsonPropertyName("tag_string")]
    public string TagString { get; set; } = string.Empty;

    [JsonPropertyName("fav_count")]
    public int? FavCount { get; set; }

    [JsonPropertyName("file_ext")]
    public string FileExt { get; set; } = string.Empty;

    [JsonPropertyName("last_noted_at")]
    public DateTime? LastNotedAt { get; set; }

    [JsonPropertyName("parent_id")]
    public int? ParentId { get; set; }

    [JsonPropertyName("has_children")]
    public bool? HasChildren { get; set; }

    [JsonPropertyName("approver_id")]
    public int? ApproverId { get; set; }

    [JsonPropertyName("tag_count_general")]
    public int? TagCountGeneral { get; set; }

    [JsonPropertyName("tag_count_artist")]
    public int? TagCountArtist { get; set; }

    [JsonPropertyName("tag_count_character")]
    public int? TagCountCharacter { get; set; }

    [JsonPropertyName("tag_count_copyright")]
    public int? TagCountCopyright { get; set; }

    [JsonPropertyName("file_size")]
    public int? FileSize { get; set; }

    [JsonPropertyName("up_score")]
    public int? UpScore { get; set; }

    [JsonPropertyName("down_score")]
    public int? DownScore { get; set; }

    [JsonPropertyName("is_pending")]
    public bool? IsPending { get; set; }

    [JsonPropertyName("is_flagged")]
    public bool? IsFlagged { get; set; }

    [JsonPropertyName("is_deleted")]
    public bool? IsDeleted { get; set; }

    [JsonPropertyName("tag_count")]
    public int? TagCount { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("is_banned")]
    public bool? IsBanned { get; set; }

    [JsonPropertyName("pixiv_id")]
    public int? PixivId { get; set; }

    [JsonPropertyName("last_commented_at")]
    public DateTime? LastCommentedAt { get; set; }

    [JsonPropertyName("has_active_children")]
    public bool? HasActiveChildren { get; set; }

    [JsonPropertyName("bit_flags")]
    public int? BitFlags { get; set; }

    [JsonPropertyName("tag_count_meta")]
    public int? TagCountMeta { get; set; }

    [JsonPropertyName("has_large")]
    public bool? HasLarge { get; set; }

    [JsonPropertyName("has_visible_children")]
    public bool? HasVisibleChildren { get; set; }

    [JsonPropertyName("media_asset")]
    public MediaAsset? MediaAsset { get; set; }

    [JsonPropertyName("tag_string_general")]
    public string TagStringGeneral { get; set; } = string.Empty;

    [JsonPropertyName("tag_string_character")]
    public string TagStringCharacter { get; set; } = string.Empty;

    [JsonPropertyName("tag_string_copyright")]
    public string TagStringCopyright { get; set; } = string.Empty;

    [JsonPropertyName("tag_string_artist")]
    public string TagStringArtist { get; set; } = string.Empty;

    [JsonPropertyName("tag_string_meta")]
    public string TagStringMeta { get; set; } = string.Empty;

    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [JsonPropertyName("large_file_url")]
    public string LargeFileUrl { get; set; } = string.Empty;

    [JsonPropertyName("preview_file_url")]
    public string PreviewFileUrl { get; set; } = string.Empty;
}
