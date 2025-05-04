using DiscordBot.Models.Dependencies;
using RestSharp;
using Serilog;
using System.IO.Compression;

namespace DiscordBot.Business.Helpers;

internal static class DependencyHelper
{
    internal static async Task LoadMissingAsync()
    {
        const string githubUrl = "https://github.com";

        if (OperatingSystem.IsLinux())
        {
            Log.Verbose("You're running this application on linux. " +
                "The dependencies should have been installed by the docekr image. " +
                "If you encounter errors try adding them manually: {one} {two} {three}", "FFMPEG", "opus", "libsodium");
        }
        else if (OperatingSystem.IsWindows())
        {
            if (File.Exists("libopus.dll") && File.Exists("libsodium.dll"))
            {
                Log.Verbose("Both dependencies {one} and {two} exist. Check for {three} yourself.", "libopus.dll", "libsodium.dll", "FFMPEG");
                return;
            }

            Log.Information("Downloading dependencies...");
            var restClient = new RestClient(githubUrl);
            var fileDownloadRequest = new RestRequest("/discord-net/Discord.Net/raw/refs/heads/dev/voice-natives/vnext_natives_win32_x64.zip");
            var fileDownloadStream = await restClient.DownloadStreamAsync(fileDownloadRequest);
            if (fileDownloadStream == null)
            {
                Log.Warning("Failed to stream from archive {host}{path}.", githubUrl, fileDownloadRequest.Resource);
                return;
            }

            using var zip = new ZipArchive(fileDownloadStream, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries.Where(w => w.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
            {
                entry.ExtractToFile(entry.Name);
                Log.Information("Extracted dependency {file}.", entry.Name);
            }

            Log.Information("Done getting dependencies.");
        }
        else
            Log.Information("Running on unsupported operating system. You'll have to handle dependencies yourself.");
    }

    private static async Task<bool> DownloadGithubReleaseAsync(
        RestClient restClient,
        string gitHubName,
        string gitHubRepo,
        string fileExtension
    )
    {
        var request = new RestRequest($"/repos/{gitHubName}/{gitHubRepo}/releases/latest");
        var response = await restClient.ExecuteAsync<Release>(request);
        var fileToDownload = response.Data?.Assets.FirstOrDefault(f => f.Name.EndsWith(fileExtension));
        if (fileToDownload == null)
            return false;

        var downloadRequest = new RestRequest(fileToDownload.BrowserDownloadUrl);
        var result = await restClient.DownloadDataAsync(downloadRequest);
        if (result == null || result.Length == 0)
            return false;

        await File.WriteAllBytesAsync(fileToDownload.Name, result);
        return true;
    }
}