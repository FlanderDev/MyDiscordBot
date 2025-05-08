using System.Diagnostics;
using CliWrap;
using Serilog;

namespace DiscordBot.Business.Helpers;
internal static class DownloadHelper
{
    internal static async Task<string?> DownloadYouTubeMediaAsync(bool requiresVideo, string url, string fileNamePrefix = "", TimeSpan? start = null, TimeSpan? end = null)
    {
        try
        {
            var directory = Directory.CreateDirectory("YouTubeMedia");
            var fileName = string.Join(string.Empty, fileNamePrefix, Guid.NewGuid(), requiresVideo ? ".mp4" : ".mp3");
            var filePath = Path.Combine(directory.FullName, fileName);

            var ytDlp = Cli.Wrap("yt-dlp")
                .WithArguments(["--download-sections", $"*{start?.ToString("g")}-{end?.ToString("g")}"])
                .WithArguments(["-o", filePath]);

            if (!requiresVideo)
                ytDlp = ytDlp.WithArguments(["-x", "--audio-format", "mp3", "--audio-quality", "0"]);

            var result = await ytDlp
                .WithArguments(url)
                .ExecuteAsync();

            return result.IsSuccess ? filePath : null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not download file.");
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
