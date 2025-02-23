using Discord;
using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBot.Models.Enteties;

public sealed class DiscordUser : IUser
{
    [Key]
    public ulong Id { get; set; }
    public string AvatarId { get; set; } = string.Empty;

    public string Discriminator { get; set; } = string.Empty;

    public ushort DiscriminatorValue { get; set; }

    public bool IsBot { get; set; }

    public bool IsWebhook { get; set; }

    public string Username { get; set; } = string.Empty;

    public UserProperties? PublicFlags { get; set; }

    public string GlobalName { get; set; } = string.Empty;

    public string AvatarDecorationHash { get; set; } = string.Empty;

    public ulong? AvatarDecorationSkuId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }


    public string Mention { get; set; } = string.Empty;

    public UserStatus Status { get; set; }

    [NotMapped]
    public IReadOnlyCollection<ClientType> ActiveClients { get; set; } = [];
    [NotMapped]
    public IReadOnlyCollection<IActivity> Activities { get; set; } = [];

    public Task<IDMChannel> CreateDMChannelAsync(RequestOptions options)
    {
        throw new NotImplementedException();
    }

    public string GetAvatarDecorationUrl()
    {
        throw new NotImplementedException();
    }

    public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
    {
        throw new NotImplementedException();
    }

    public string GetDefaultAvatarUrl()
    {
        throw new NotImplementedException();
    }

    public string GetDisplayAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
    {
        throw new NotImplementedException();
    }

    public static DiscordUser FromSocketUser(SocketUser socketUser) => new()
    {
        Id = socketUser.Id,
        ActiveClients = socketUser.ActiveClients,
        Activities = socketUser.Activities,
        AvatarDecorationHash = socketUser.AvatarDecorationHash,
        AvatarId = socketUser.AvatarId,
        Discriminator = socketUser.Discriminator,
        GlobalName = socketUser.GlobalName,
        Mention = socketUser.Mention,
        Status = socketUser.Status,
        Username = socketUser.Username,
    };
}
