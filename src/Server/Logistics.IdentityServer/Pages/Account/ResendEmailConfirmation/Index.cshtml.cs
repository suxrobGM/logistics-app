using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using Logistics.Application.Core.Services;
using Logistics.Domain.Entities;

namespace Logistics.IdentityServer.Pages.Account.ResendEmailConfirmation;

[AllowAnonymous]
public class ResendEmailConfirmationModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSenderService;

    public ResendEmailConfirmationModel(UserManager<User> userManager, IEmailSender emailSenderService)
    {
        _userManager = userManager;
        _emailSenderService = emailSenderService;
    }

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

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new { userId = userId, code = code },
            Request.Scheme);
        await _emailSenderService.SendEmailAsync(
            Input.Email,
            "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
        return Page();
    }
}
