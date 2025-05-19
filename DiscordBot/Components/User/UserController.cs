using DiscordBot.Business.Helpers.Blazor;
using DiscordBot.Models.Internal.Configs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;
using Serilog;
using System.Security.Claims;
using DiscordBot.Models.Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Components.User;
public sealed class UserController([FromServices] IHttpContextAccessor httpContextAccessor) : Controller
{
    [Route("/User/DiscordOAuth2")]
    public async Task<IActionResult> OnLoginDiscordAsync([FromServices] IOptions<Configuration> options)
    {
        try
        {
            if (httpContextAccessor.HttpContext == null)
            {
                Log.Error("No valid context is available.");
                return Redirect(RouteHelper.Error);
            }

            var authorizationCode = httpContextAccessor.HttpContext.Request.Query.TryGetValue("code", out var value) ? value.ToString() : null;
            if (string.IsNullOrWhiteSpace(authorizationCode))
            {
                Log.Error("No valid discord code!");
                return Redirect(RouteHelper.Error);
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
                return Redirect(RouteHelper.Error);
            }

            var accessToken = (JsonConvert.DeserializeObject(restResponse.Content ?? string.Empty) as JObject)?["access_token"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                Log.Error("Could not deserialize response and get the access token.");
                return Redirect(RouteHelper.Error);
            }

            var infoRequest = new RestRequest("/api/users/@me").AddHeader("Authorization", $"Bearer {accessToken}");
            var response = await restClient.ExecuteAsync<UserSelf>(infoRequest);
            if (response.Data == null)
            {
                Log.Error("Failed to retrieve user data.");
                return Redirect(RouteHelper.Error);
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Sid, response.Data.Id), new Claim(ClaimTypes.Name, response.Data.Username)], CookieAuthenticationDefaults.AuthenticationScheme));
            await httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return Redirect(RouteHelper.Home);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error authentication via discord.");
            return Redirect(RouteHelper.Error);
        }
    }

    [Route("/User/Login")]
    public async Task<IActionResult> OnLoginAsync()
    {
        if (httpContextAccessor.HttpContext == null)
        {
            Log.Warning("No context to login off.");
            return Redirect(RouteHelper.Home);
        }

        await httpContextAccessor.HttpContext.SignOutAsync();
        return Redirect(RouteHelper.Home);
    }

    [Route("/User/Logout")]
    public async Task<IActionResult> OnLogoutAsync()
    {
        if (httpContextAccessor.HttpContext == null)
        {
            Log.Warning("No context to logout off.");
            return Redirect(RouteHelper.Home);
        }

        var username = httpContextAccessor.HttpContext.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            Log.Verbose("No user to logout.");
            return Redirect(RouteHelper.Home);
        }

        Log.Verbose("Logging out '{username}'.", username);
        await httpContextAccessor.HttpContext.SignOutAsync();
        return Redirect(RouteHelper.Home);
    }
}
