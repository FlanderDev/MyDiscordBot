using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Business.Bots;

internal sealed class KumoBot(IServiceProvider serviceProvider, char? prefix = null) : BotBase(serviceProvider, prefix)
{
}
