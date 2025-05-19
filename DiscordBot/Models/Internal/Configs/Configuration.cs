namespace DiscordBot.Models.Internal.Configs;

public sealed class Configuration
{
    public Discord Discord { get; set; } = new();
    public Danbooru Danbooru { get; set; } = new();
    public Blazor Blazor { get; set; } = new();
}