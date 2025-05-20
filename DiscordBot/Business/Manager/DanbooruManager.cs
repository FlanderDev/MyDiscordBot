using DiscordBot.Models.Danbooru;
using DiscordBot.Models.Internal.Configs;
using RestSharp;
using Serilog;
using System.Text.Json;

namespace DiscordBot.Business.Manager;

public sealed class DanbooruManager(Danbooru danbooru)
{
    private const string DomainAddress = "https://danbooru.donmai.us";
    private RestRequest GetLoginRequest(string resource)
                    => new RestRequest(resource)
                        .AddQueryParameter("login", danbooru.Username)
                        .AddQueryParameter("api_key", danbooru.Token);


    internal Task<(string? ImageUrl, int ImageIndex)> GetRandomImageByTagAsync(params string[] tags) => GetRandomImageByTagAsync(-1, tags);
    internal async Task<(string? ImageUrl, int ImageIndex)> GetRandomImageByTagAsync(int setIndex = -1, params string[] tags)
    {
        try
        {
            if (tags.Length > 6)
                throw new Exception("Cannot search for more than six tags.");

            var parameterTags = string.Join(string.Empty, tags);
            var restClient = new RestClient(DomainAddress);
            var request = GetLoginRequest("/posts.json")
                         .AddQueryParameter("tags", parameterTags, false);

            var restResult = await restClient.ExecuteAsync(request);
            if (restResult.Content == null)
            {
                Log.Debug("You used an invalid tag, used tags: {tags}", parameterTags);
                return (null, -1);
            }

            var posts = JsonSerializer.Deserialize<List<PostResponse>>(restResult.Content);
            if (posts == null)
            {
                Log.Warning("Parsing error: '{content}'.", restResult.Content);
                return (null, -1);
            }

            posts = [.. posts.Where(w => !w.Rating.Equals("e"))];

            if (posts.Count == 0)
            {
                Log.Warning("No results for danbooru collection, with tags: {tags}", parameterTags);
                return (null, -1);
            }

            var index = setIndex != -1 && posts.Count > setIndex ? setIndex : Random.Shared.Next(0, posts.Count);
            var randomResult = posts[index];

            Log.Debug("Returing result for tags '{tags}'.", parameterTags);
            return (randomResult.FileUrl, index);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexped error.");
            return (null, -1);
        }
    }
}

