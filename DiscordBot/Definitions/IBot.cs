using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Definitions;
public interface IBot
{
    Task<bool> StartAsync(ServiceProvider services, string? botToken, string? name);
    Task<bool> StopAsync();
}
