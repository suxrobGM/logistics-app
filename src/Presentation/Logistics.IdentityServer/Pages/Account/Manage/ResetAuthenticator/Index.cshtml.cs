using Logistics.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account.Manage.ResetAuthenticator;

public class ResetAuthenticatorModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<ResetAuthenticatorModel> logger) : PageModel
{
    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        return user is null
            ? NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.")
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        logger.LogInformation("User with ID '{UserId}' has reset their authenticator app key.", user.Id);

        // The key is part of the security stamp; refresh so this session isn't signed out.
        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your authenticator app key has been reset. Set up the app again with the new key.";

        return RedirectToPage("/Account/Manage/EnableAuthenticator/Index");
    }
}
