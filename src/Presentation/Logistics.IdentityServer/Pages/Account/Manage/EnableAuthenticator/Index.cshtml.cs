using System.Text;
using System.Text.Encodings.Web;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

namespace Logistics.IdentityServer.Pages.Account.Manage.EnableAuthenticator;

public class EnableAuthenticatorModel(
    UserManager<User> userManager,
    ILogger<EnableAuthenticatorModel> logger) : PageModel
{
    private const int RecoveryCodeCount = 10;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string SharedKey { get; private set; } = string.Empty;

    /// <summary>The <c>otpauth://</c> URI rendered as an inline SVG QR code.</summary>
    public string QrCodeSvg { get; private set; } = string.Empty;

    [TempData]
    public string[]? RecoveryCodes { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        await LoadSharedKeyAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSharedKeyAsync(user);
            return Page();
        }

        var code = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, code);

        if (!isValid)
        {
            ModelState.AddModelError("Input.Code", "Verification code is invalid.");
            await LoadSharedKeyAsync(user);
            return Page();
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", user.Id);

        StatusMessage = "Your authenticator app has been verified.";
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount);
        RecoveryCodes = recoveryCodes?.ToArray();
        return RedirectToPage("/Account/Manage/ShowRecoveryCodes/Index");
    }

    private async Task LoadSharedKeyAsync(User user)
    {
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        }

        SharedKey = FormatKey(unformattedKey!);

        var email = await userManager.GetEmailAsync(user);
        var uri = BuildAuthenticatorUri(email!, unformattedKey!);
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
        QrCodeSvg = new SvgQRCode(data).GetGraphic(4, "#000000", "#ffffff", true, SvgQRCode.SizingMode.ViewBoxAttribute);
    }

    /// <summary>Groups the base32 key in fours so it can be typed by hand.</summary>
    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        for (var i = 0; i < unformattedKey.Length; i += 4)
        {
            result.Append(unformattedKey.AsSpan(i, Math.Min(4, unformattedKey.Length - i))).Append(' ');
        }

        return result.ToString().TrimEnd().ToLowerInvariant();
    }

    /// <summary>Key URI format understood by Google Authenticator, Microsoft Authenticator, 1Password and the rest.</summary>
    private static string BuildAuthenticatorUri(string email, string unformattedKey)
    {
        var issuer = UrlEncoder.Default.Encode(ProductInfo.Name);
        return $"otpauth://totp/{issuer}:{UrlEncoder.Default.Encode(email)}?secret={unformattedKey}&issuer={issuer}&digits=6";
    }
}
