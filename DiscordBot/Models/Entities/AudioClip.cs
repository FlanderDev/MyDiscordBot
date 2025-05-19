using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Models.Entities;

public sealed class AudioClip
{
    [Key]
    public int Id { get; set; }
    public ulong DiscordUserId { get; set; }
    [MaxLength(50)]
    public string CallCode { get; set; } = string.Empty;
    [MaxLength(999)]
    public string FileName { get; set; } = string.Empty;

    public override string ToString() => $"{Id} - {DiscordUserId} - '{CallCode}'";
}
