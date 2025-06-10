using DiscordBot.Models.Discord;
using DiscordBot.Models.Internal.Configs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.Business.Helpers.Blazor;

public sealed class LoginService([FromServices] IHttpContextAccessor httpContextAccessor, IOptions<Configuration> options)
{
    internal readonly string DiscordAuthUrl =
        new StringBuilder("https://discord.com/api/oauth2/authorize?client_id=")
            .Append(options.Value.Discord.ClientId)
            .Append("&redirect_uri=")
            .Append(Uri.EscapeDataString(options.Value.Discord.RedirectUri))
            .Append("&response_type=code&scope=")
            .Append(Uri.EscapeDataString(options.Value.Discord.Scopes))
            .ToString();

    internal bool UserIsLoggedIn => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    internal string? GetDiscordOAuthCodeFromQuery()
    {
        if (httpContextAccessor.HttpContext == null)
        {
            Log.Warning("No context to operate on.");
            return null;
        }

        var authorizationCode = httpContextAccessor.HttpContext.Request.Query.TryGetValue("code", out var value) ? value.ToString() : null;
        if (string.IsNullOrWhiteSpace(authorizationCode))
            return null;

        Log.Verbose("Got auth-code '{code}'.", authorizationCode);
        return authorizationCode;
    }

    internal async Task<bool> LoginUserAsync(string authorizationCode)
    {
        try
        {
            if (httpContextAccessor.HttpContext == null)
            {
                Log.Warning("No context to operate on.");
                return false;
            }

            var discord = options.Value.Discord;
            var restClient = new RestClient("https://discord.com");
            var authRequest = new RestRequest("/api/oauth2/token", Method.Post)
                .AddParameter("client_id", discord.ClientId)
                .AddParameter("client_secret", discord.ClientSecret)
                .AddParameter("grant_type", "authorization_code")
                .AddParameter("code", authorizationCode)
                .AddParameter("redirect_uri", discord.RedirectUri);
            var restResponse = await restClient.ExecuteAsync(authRequest);
            if (!restResponse.IsSuccessful)
            {
                Log.Error("Bad response from discord OAuth2 token validation.");
                return false;
            }

            var accessToken = (JsonConvert.DeserializeObject(restResponse.Content ?? string.Empty) as JObject)?["access_token"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                Log.Error("Could not deserialize response and get the access token.");
                return false;
            }
            Log.Verbose("Got access-token '{token}'.", accessToken);

            var infoRequest = new RestRequest("/api/users/@me").AddHeader("Authorization", $"Bearer {accessToken}");
            var response = await restClient.ExecuteAsync<UserSelf>(infoRequest);
            if (response.Data == null)
            {
                Log.Error("Failed to retrieve user data.");
                return true;
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Sid, response.Data.Id), new Claim(ClaimTypes.Name, response.Data.Username)], CookieAuthenticationDefaults.AuthenticationScheme));
            await httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error authentication via discord.");
            return false;
        }
    }

    /// <summary>
    /// Logs out the user.
    /// </summary>
    /// <returns>A <see cref="bool"/> indicating success or failure.</returns>
    /// <remarks>If there is no user to logout, it also returns <see langword="true"/></remarks>
    internal async Task<bool> LogoutUserAsync()
    {
        try
        {
            if (httpContextAccessor.HttpContext == null)
            {
                Log.Warning("No context to operate on.");
                return false;
            }

            var username = httpContextAccessor.HttpContext.User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
            {
                Log.Warning("No identity to logout.");
                return true;
            }

            Log.Verbose("Logging out '{username}'.", username);
            await httpContextAccessor.HttpContext.SignOutAsync();
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error logging out user.");
            return false;
        }
    }
}
