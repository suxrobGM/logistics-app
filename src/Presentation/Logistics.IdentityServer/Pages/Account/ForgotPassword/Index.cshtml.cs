using System.Text;
using System.Text.Encodings.Web;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Logistics.IdentityServer.Pages.Account.ForgotPassword;

[AllowAnonymous]
public class ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("../ForgotPasswordConfirmation");
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword/Index",
            null,
            new { code },
            Request.Scheme)
                ?? throw new InvalidOperationException("Could not generate password reset callback URL.");

        var encodedUrl = HtmlEncoder.Default.Encode(callbackUrl);
        await emailSender.SendEmailAsync(
            Input.Email,
            "Reset Password",
            $"Please reset your password by <a href='{encodedUrl}'>clicking here</a>.");

        return RedirectToPage("../ForgotPasswordConfirmation");
    }
}
