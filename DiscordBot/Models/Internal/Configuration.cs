namespace DiscordBot.Models.Internal;

public sealed class Configuration
{
    public Discord Discord { get; set; } = new();

    public string DanbooruToken { get; set; } = string.Empty;
}