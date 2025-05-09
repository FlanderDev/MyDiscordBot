using System;
using System.Diagnostics;
using System.Text;
using Serilog;

namespace DiscordBot.Business.Helpers;

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
                var process = Process.Start("chmod", "+x yt-dlp");
                await process.WaitForExitAsync();

                var info = await process.StandardOutput.ReadToEndAsync();
                Log.Information(info);

                var error = await process.StandardError.ReadToEndAsync();
                Log.Warning(error);
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

    internal static async Task<(string, string)> UpdateYtDlpAsync()
    {
        var psi = new ProcessStartInfo("yt-dlp")
        {
            Arguments = "-U",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };
        var process = Process.Start(psi) ?? throw new Exception("Process could not be generated.");
        await process.WaitForExitAsync();

        var info = await process.StandardOutput.ReadToEndAsync();
        Log.Information(info);

        var error = await process.StandardError.ReadToEndAsync();
        Log.Warning(error);

        Log.Information("UPDATE DONE!");
        return (info, error);
    }

    internal static async Task<string?> DownloadYouTubeMediaAsync(bool requiresVideo, string url, string fileNamePrefix = "", TimeSpan? start = null, TimeSpan? end = null)
    {
        if (!await EnsureDownloaderExistsAsync())
        {
            Log.Warning("Downloader could not located nor downloaded, aborting.");
            return null;
        }

        var directory = Directory.CreateDirectory("YouTubeMedia");
        var fileName = string.Join(string.Empty, fileNamePrefix, '-', Guid.NewGuid());
        var filePath = Path.Combine(directory.Name, fileName);
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

            //ytDlp = ytDlp.WithArguments(["-x", "--audio-format", "mp3", "--audio-quality", "0"]);
            var rawArguments = string.Join(' ', arguments);

            var psi = new ProcessStartInfo("yt-dlp")
            {
                Arguments = rawArguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            var process = Process.Start(psi);
            if (process == null)
            {
                Log.Error("Could not create process for {fileName}.", psi.FileName);
                return null;
            }

            await process.WaitForExitAsync();

            var output = await process.StandardOutput.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(output))
                Log.Information(output);

            var error = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(error))
                Log.Error(error);

            var fullFileName = directory.GetFiles($"{fileName}*").FirstOrDefault()?.FullName;
            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(fullFileName) ? fullFileName : null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not download YouTube media with arguments: {arguments}", arguments);
            return null;
        }
    }
}
