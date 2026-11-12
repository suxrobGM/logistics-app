using Logistics.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Telegram;

[SecurityHeaders]
[Authorize]
public class Login(TelegramLoginService telegramLoginService) : PageModel
{
    public string? ErrorMessage { get; set; }
    public string? TenantName { get; set; }
    public string? UserName { get; set; }
    public string? RoleDisplay { get; set; }
    public bool Success { get; set; }

    public async Task<IActionResult> OnGet([FromQuery] string? state, CancellationToken ct)
    {
        var result = await telegramLoginService.ConnectChatAsync(state, User, ct);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error;
            return Page();
        }

        var connection = result.Value!;
        Success = true;
        TenantName = connection.TenantName;
        UserName = connection.UserName;
        RoleDisplay = connection.RoleDisplay;
        return Page();
    }
}
