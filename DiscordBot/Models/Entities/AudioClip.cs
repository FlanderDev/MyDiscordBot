using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Models.Entities;

[Table("AudioClips")]
public sealed class AudioClip
{
    [Key]
    public int Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public DiscordUser? DiscordUser { get; set; }

    [MaxLength(50)]
    public string CallCode { get; set; } = string.Empty;

    [MaxLength(999)]
    public string FilePath { get; set; } = string.Empty;

    public override string ToString() => $"{Id} - {DiscordUserId} - '{CallCode}'";
        
}
