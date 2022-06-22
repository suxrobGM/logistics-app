using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account
{
    [AllowAnonymous]
    public class Lockout : PageModel
    {
        public void OnGet()
        {

        }
    }
}
