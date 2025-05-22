using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Models.Entities;

[Table("DiscordUsers")]
public sealed class DiscordUser
{
    [Key]
    public ulong Id { get; set; }

    [MaxLength(250)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(250)]
    public string GlobalName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Mention { get; set; } = string.Empty;

    public bool Administrator { get; set; }

    public static DiscordUser FromSocketUser(SocketUser socketUser) => new()
    {
        Id = socketUser.Id,
        GlobalName = socketUser.GlobalName,
        Mention = socketUser.Mention,
        Username = socketUser.Username,
    };
}
