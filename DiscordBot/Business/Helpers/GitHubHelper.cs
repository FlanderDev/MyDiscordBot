using DiscordBot.Models.Dependencies;
using RestSharp;

namespace DiscordBot.Business.Helpers;
internal static class GitHubHelper
{
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
