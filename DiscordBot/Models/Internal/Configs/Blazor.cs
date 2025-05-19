namespace DiscordBot.Models.Internal.Configs;

public sealed class Blazor
{
    /// <summary>
    /// Defaults to 100MB, can be overwritten via configuration.
    /// </summary>
    public int FileSizeLimitMb { get; set; } = 100;
}
