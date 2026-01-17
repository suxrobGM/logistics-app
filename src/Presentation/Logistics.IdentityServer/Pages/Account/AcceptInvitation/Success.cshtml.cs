using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account.AcceptInvitation;

[AllowAnonymous]
public class Success : PageModel
{
    public string? TenantName { get; set; }
    public string? Role { get; set; }

    public void OnGet(string? tenantName, string? role)
    {
        TenantName = tenantName;
        Role = role;
    }
}
