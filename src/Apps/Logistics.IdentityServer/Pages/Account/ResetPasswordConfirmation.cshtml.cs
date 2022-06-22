using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordConfirmation : PageModel
    {
        public void OnGet()
        {

        }
    }
}
