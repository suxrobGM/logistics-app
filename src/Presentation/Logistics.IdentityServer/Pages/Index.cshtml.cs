using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Home;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    public bool IsSignedIn => User.Identity?.IsAuthenticated == true;
}
