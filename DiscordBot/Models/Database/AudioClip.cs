using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Models.Database;

public class AudioClip
{
    [Key]
    public int Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public string CallCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
