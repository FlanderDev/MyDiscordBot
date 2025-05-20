using Serilog;
using System.Text;

namespace DiscordBot.Business.Helpers.Bot;

internal static class DownloadHelper
{
    internal static async Task<bool> EnsureDownloaderExistsAsync(bool forceUpdate = false)
    {
        try
        {
            if (forceUpdate && File.Exists("yt-dlp"))
                File.Delete("yt-dlp");

            if (File.Exists("yt-dlp"))
                return true;

            Log.Verbose("Downloading yt-dlp...");
            var fileName = OperatingSystem.IsLinux() ? "yt-dlp_linux" : "yt-dlp.exe";
            var result = await GitHubHelper.DownloadGithubReleaseAsync("yt-dlp", "yt-dlp", fileName, "yt-dlp");
            if (result && OperatingSystem.IsLinux())
            {
                if (await ProcessHelper.StartProcessAsync("chmod", "+x yt-dlp") is not { } value)
                {
                    Log.Error("Could not create process for {fileName}.", "chmod");
                    return false;
                }
            }

            Log.Information("Downloading yt-dlp; {result}.", result ? "successful" : "failure");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected failure while ensuring yt-dlp's existence.");
            return false;
        }
    }

    internal static async Task<bool> UpdateYtDlpAsync()
    {
        var value = await ProcessHelper.StartProcessAsync("yt-dlp", "-U");
        return value?.info != null;
    }

    internal static async Task<string?> DownloadYouTubeMediaAsync(bool requiresVideo, string url, string fileNamePrefix = "", TimeSpan? start = null, TimeSpan? end = null)
    {
        if (!await EnsureDownloaderExistsAsync())
        {
            Log.Warning("Downloader could not located nor downloaded, aborting.");
            return null;
        }

        var mediaDirectory = FileHelper.GetMediaDirectory();
        if (!Directory.Exists(mediaDirectory))
            Directory.CreateDirectory(mediaDirectory);

        var fileName = string.Join(string.Empty, fileNamePrefix, '-', Guid.NewGuid());
        var filePath = Path.Combine(mediaDirectory, fileName);
        List<string> arguments = [url, "-o", filePath];
        try
        {
            if (!requiresVideo)
                arguments.Add("-x");

            if (start != null || end != null)
            {
                arguments.Add("--download-sections");

                var timeRange = new StringBuilder("*");
                if (start != null)
                    timeRange.Append(start.Value.ToString("g"));

                timeRange.Append('-');

                if (end != null)
                    timeRange.Append(end.Value.ToString("g"));

                arguments.Add(timeRange.ToString());
            }

            if (await ProcessHelper.StartProcessAsync("yt-dlp", arguments) is not { } value)
            {
                Log.Error("Could not create process for {fileName}.", "yt-dlp");
                return null;
            }

            var fullFileName = Directory.GetFiles(mediaDirectory, $"{fileName}*").FirstOrDefault();
            return value.info != null && string.IsNullOrWhiteSpace(fullFileName) ? null : fullFileName;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not download YouTube media with arguments: {arguments}", arguments);
            return null;
        }
    }
}
