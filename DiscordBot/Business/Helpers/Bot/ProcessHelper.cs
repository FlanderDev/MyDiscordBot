using Serilog;
using System.Diagnostics;

namespace DiscordBot.Business.Helpers.Bot;
internal static class ProcessHelper
{
    internal static async Task<(string? info, string? error)?> StartProcessAsync(string fileName, params IEnumerable<string> arguments)
    {
        var rawArguments = string.Join(' ', arguments);
        try
        {
            var psi = new ProcessStartInfo(fileName)
            {
                Arguments = rawArguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            var process = Process.Start(psi) ?? throw new Exception("Process could not be created.");
            await process.WaitForExitAsync(new CancellationTokenSource(new TimeSpan(0,3,0)).Token);

            var info = await process.StandardOutput.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(info))
                Log.Information(info);

            var error = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(error))
                Log.Warning(error);

            return (info, error);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing process for process '{name}' with args '{args}'.", fileName, rawArguments);
            return null;
        }
    }
}
