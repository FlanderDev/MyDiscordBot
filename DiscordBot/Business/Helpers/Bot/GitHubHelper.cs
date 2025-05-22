using DiscordBot.Models.Dependencies;
using RestSharp;
using Serilog;

namespace DiscordBot.Business.Helpers.Bot;
internal static class GitHubHelper
{
    internal static async Task<bool> DownloadGithubReleaseAsync(
        string gitHubName,
        string gitHubRepo,
        string fileEndingMatch,
        string? saveName = null
    )
    {
        try
        {
            var restClient = new RestClient("https://api.github.com");
            var request = new RestRequest($"/repos/{gitHubName}/{gitHubRepo}/releases/latest");
            var response = await restClient.ExecuteAsync<Release>(request);
            var fileToDownload = response.Data?.Assets.FirstOrDefault(f => f.Name.EndsWith(fileEndingMatch));
            if (fileToDownload == null)
                return false;

            var downloadRequest = new RestRequest(fileToDownload.BrowserDownloadUrl);
            var result = await restClient.DownloadDataAsync(downloadRequest);
            if (result == null || result.Length == 0)
                return false;

            await File.WriteAllBytesAsync(saveName ?? fileToDownload.Name, result);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error downloading release from github.");
            return false;
        }
    }
}
