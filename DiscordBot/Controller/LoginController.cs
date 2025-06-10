using DiscordBot.Business.Helpers.Blazor;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.Controller;

[Route("/User/oAuth/")]
[ApiController]
public sealed class LoginController : ControllerBase
{
    [Route("Discord")]
    public async Task<IActionResult> DiscordAuthenticationAsync(
        [FromServices] LoginService loginService,
        [FromQuery(Name = "code")] string code)
    {
        try
        {
            var result = await loginService.LoginUserAsync(code);
            return Redirect(result ? RouteHelper.Home : RouteHelper.Error);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
