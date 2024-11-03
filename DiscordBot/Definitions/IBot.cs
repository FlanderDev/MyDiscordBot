using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Definitions;
public interface IBot
{
    Task StartAsync(ServiceProvider services);
    Task StopAsync();
}
