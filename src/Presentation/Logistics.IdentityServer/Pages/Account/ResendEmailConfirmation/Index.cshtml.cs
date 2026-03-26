using System.Text;
using Logistics.Application.Contracts.Models.Email;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Logistics.IdentityServer.Pages.Account.ResendEmailConfirmation;

[AllowAnonymous]
public class ResendEmailConfirmationModel(
    UserManager<User> userManager,
    IEmailSender emailSenderService,
    IEmailTemplateService emailTemplateService) : PageModel
{

    [BindProperty]
    public InputModel Input { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new { userId, code },
            Request.Scheme);

        var model = new EmailConfirmationEmailModel { ConfirmUrl = callbackUrl! };
        var body = await emailTemplateService.RenderAsync("EmailConfirmation", model);
        await emailSenderService.SendEmailAsync(Input.Email, "Confirm your email", body);

        ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
        return Page();
    }
}
