using RestSharp;
using Serilog;

namespace DiscordBot.Business.Manager;

internal static class FileManager
{
    internal static async Task<string?> GetLocalReosurceOrDownloadAsync(string fileName, string url)
    {
        try
        {
            if (File.Exists(fileName))
            {
                Log.Debug("Using local file.");
                return fileName;
            }
            Log.Debug("Downloading file");

            var restClient = new RestClient();
            var restRequest = new RestRequest(url);
            var data = await restClient.DownloadDataAsync(restRequest);
            if (data == null)
            {
                Log.Warning("Could not download resource.");
                return null;
            }

            File.WriteAllBytes(fileName, data);
            Log.Debug("Downloaded resouce.");
            return fileName;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not locate resource.");
            return null;
        }
    }
}
