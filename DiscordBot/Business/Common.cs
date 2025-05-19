namespace DiscordBot.Business;
internal static class Common
{
    internal static CancellationTokenSource CancellationTokenSource = new();
    internal static CancellationTokenSource SetNewCancelSource() => CancellationTokenSource = new CancellationTokenSource();
}
