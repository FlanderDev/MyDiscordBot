using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Models.Enteties;

public sealed class AudioClip
{
    [Key]
    public int Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public string CallCode { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public override string ToString() => $"{Id} - {DiscordUserId} - '{CallCode}'";
}
