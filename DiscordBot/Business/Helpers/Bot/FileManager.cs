using RestSharp;
using Serilog;

namespace DiscordBot.Business.Helpers.Bot;

internal static class FileManager
{
    internal static DirectoryInfo GetDatabaseDirectory() => new("Database");
    internal static DirectoryInfo GetFileUploadDirectory() => new("wwwroot/uploads");
    internal static DirectoryInfo GetMediaDirectory() => new("MediaFiles");

    internal static async Task<string?> GetLocalResourceOrDownloadAsync(string fileName, string url)
    {
        try
        {
            if (File.Exists(fileName))
            {
                Log.Debug("Using local file '{fileName}'.", fileName);
                return fileName;
            }
            Log.Debug("Downloading file at '{url}'.", url);

            var restClient = new RestClient();
            var restRequest = new RestRequest(url);
            var data = await restClient.DownloadDataAsync(restRequest);
            if (data == null)
            {
                Log.Warning("Could not download resource '{url}'.", url);
                return null;
            }

            await File.WriteAllBytesAsync(fileName, data);
            Log.Debug("Downloaded resource '{url}' to '{fileName}'.", url, fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not locate resource at '{url}'.", url);
            return null;
        }
    }
}
