using System.Diagnostics;
using System.Text;
using CliWrap;
using Serilog;

namespace DiscordBot.Business.Helpers;
internal static class DownloadHelper
{
    internal static async Task<string?> DownloadYouTubeMediaAsync(bool requiresVideo, string url, string fileNamePrefix = "", TimeSpan? start = null, TimeSpan? end = null)
    {
        var directory = Directory.CreateDirectory("YouTubeMedia");
        var fileName = string.Join(string.Empty, fileNamePrefix, '-', Guid.NewGuid());
        var filePath = Path.Combine(directory.FullName, fileName);
        List<string> arguments = [url, "-o", filePath];
        string rawArguments;
        try
        {
            if (!requiresVideo)
                arguments.Add("-x");

            if (start != null || end != null)
            {
                arguments.Add("--download-sections");

                var timeRange = new StringBuilder();
                if (start != null)
                    timeRange.Append(start.Value.ToString("g"));

                timeRange.Append('-');

                if (end != null)
                    timeRange.Append(end.Value.ToString("g"));

                arguments.Add(timeRange.ToString());
            }

            //ytDlp = ytDlp.WithArguments(["-x", "--audio-format", "mp3", "--audio-quality", "0"]);
            rawArguments = string.Join(' ', arguments);

            var psi = new ProcessStartInfo("yt-dlp")
            {
                Arguments = rawArguments,
                RedirectStandardError = true
            };
            var process = Process.Start(psi);
            if (process == null)
            {
                Log.Error("Could not create process for {fileName}.", psi.FileName);
                return null;
            }

            var error = await process.StandardError.ReadToEndAsync();
            return string.IsNullOrWhiteSpace(error) ? filePath : null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not download YouTube media with arguments: {arguments}", arguments);
            return null;
        }
    }

    private static Process DownloadYouTubeVideo(string arguments) =>
        Process.Start(new ProcessStartInfo
        {
            FileName = "yt-dlp",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        }) ?? throw new Exception("Could not initialize yt-dlp process.");
}
