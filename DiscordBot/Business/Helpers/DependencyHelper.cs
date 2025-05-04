using System.Net;
using DiscordBot.Models.Dependencies;
using FluentFTP;
using RestSharp;
using Serilog;

namespace DiscordBot.Business.Helpers;

internal static class DependencyHelper
{
    internal static async Task LoadMissingAsync()
    {
        if (!File.Exists("libsodium"))
            await DownloadLibsodiumAsync();

        if (!File.Exists("opium"))
            await DownloadOpiusAsync();

        Log.Verbose("Done checking dependencies.");
    }

    private static async Task DownloadOpiusAsync()
    {
        try
        {
            var ftpClientAsync = new AsyncFtpClient("ftp.osuosl.org");
            await ftpClientAsync.Connect();
            var listigns = (await ftpClientAsync.GetListing("/pub/xiph/releases/opus/")).OrderBy(x => x.Modified).ToList();
            
            
            var ftpClient = new FtpClient("ftp.osuosl.org");
            // ftpClient.Credentials = new NetworkCredential("anonymous", "anonymous@example.com");
            ftpClient.Connect();
            var files = ftpClient.GetListing("/pub/xiph/releases/opus/").OrderBy(x => x.Modified).ToList();
            var result = ftpClient.DownloadBytes(out var bytes, "");
            
            Log.Information("Sucessfully downladed dependency opus.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not downlaod libsodium.");
        }
    }

    private static async Task DownloadLibsodiumAsync()
    {
        try
        {
            using var githubRestClient = new RestClient("https://api.github.com");
            var resultLibsodium = await DownloadGithubReleaseAsync(
                githubRestClient,
                "jedisct1",
                "libsodium",
                OperatingSystem.IsWindows() ? ".zip" : ".tar.gz");
            
            Log.Information("Sucessfully downladed dependency libsodium.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not downlaod libsodium.");
        }
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

        await File.WriteAllBytesAsync(gitHubRepo, result);
        return true;
    }
}