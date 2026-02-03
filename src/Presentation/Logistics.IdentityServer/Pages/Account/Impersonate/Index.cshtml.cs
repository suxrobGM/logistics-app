using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Logistics.IdentityServer.Pages.Account.Impersonate;

[SecurityHeaders]
[AllowAnonymous]
[EnableRateLimiting("impersonate")]
public class Index(
    IMasterUnitOfWork masterUow,
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    IOptions<ImpersonationOptions> impersonationOptions,
    ILogger<Index> logger) : PageModel
{
    private readonly string tmsPortalUrl = impersonationOptions.Value.TmsPortalUrl;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGet(string? token, string? returnUrl)
    {
        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("Impersonation attempted without token");
            return RedirectToPage("/Account/Login/Index", new { error = "Missing impersonation token" });
        }

        // Find and validate impersonation token
        var impersonationToken = await masterUow.Repository<ImpersonationToken>()
            .GetAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        if (impersonationToken is null)
        {
            logger.LogWarning("Invalid or expired impersonation token attempted: {Token}", token[..Math.Min(10, token.Length)]);
            return RedirectToPage("/Account/Login/Index", new { error = "Invalid or expired impersonation token" });
        }

        // Mark token as used
        impersonationToken.IsUsed = true;
        impersonationToken.UsedAt = DateTime.UtcNow;
        masterUow.Repository<ImpersonationToken>().Update(impersonationToken);
        await masterUow.SaveChangesAsync();

        // Find target user
        var targetUser = await userManager.FindByIdAsync(impersonationToken.TargetUserId.ToString());
        if (targetUser is null)
        {
            logger.LogWarning("Impersonation target user not found: {UserId}", impersonationToken.TargetUserId);
            return RedirectToPage("/Account/Login/Index", new { error = "Target user not found" });
        }

        // Sign out any existing session first
        await signInManager.SignOutAsync();

        // Sign in as target user
        await signInManager.SignInAsync(targetUser, isPersistent: false);

        logger.LogInformation(
            "IMPERSONATION SUCCESSFUL: Admin {AdminId} signed in as user {TargetEmail} ({TargetId})",
            impersonationToken.AdminUserId, targetUser.Email, targetUser.Id);

        // Redirect to TMS Portal with autoLogin to trigger OIDC flow
        // The TMS Portal will see the user is already signed in at IdentityServer and get tokens
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : $"{tmsPortalUrl}?autologin=true";
        return Redirect(redirectUrl);
    }
}
