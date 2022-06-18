using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.OfficeApp.Controllers;

[Microsoft.AspNetCore.Mvc.Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    [HttpGet("Login")]
    public IActionResult Login(string? redirectUri)
    {
        if (string.IsNullOrEmpty(redirectUri))
        {
            redirectUri = Url.Content("~/");
        }

        return Challenge(
            new AuthenticationProperties { RedirectUri = redirectUri }, "oidc");
    }
    
    [HttpGet("Logout")]
    public async Task Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        await HttpContext.SignOutAsync("oidc");
    }
}
