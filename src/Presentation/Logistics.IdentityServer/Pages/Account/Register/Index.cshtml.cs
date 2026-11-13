using System.Text;
using Logistics.Application.Abstractions.Communications;
using Logistics.Application.Abstractions.Communications.EmailModels;
using Logistics.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;


namespace Logistics.IdentityServer.Pages.Account.Register;

[AllowAnonymous]
public class Index(
    IConfiguration configuration,
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    IEmailSender emailSender,
    IEmailTemplateService emailTemplateService,
    ICaptchaService captchaService,
    ILogger<Index> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }
    public string? CaptchaSiteKey { get; } = configuration.GetValue<string>("GoogleRecaptcha:SiteKey");
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = [];

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var captchaValue = HttpContext.Request.Form["g-recaptcha-response"].ToString();
        var validCaptcha = await captchaService.VerifyCaptchaAsync(captchaValue);
        ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!validCaptcha)
            ModelState.AddModelError("captcha", "Invalid captcha verification");

        if (!ModelState.IsValid)
            return Page();

        var user = new User
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName,
            LastName = Input.LastName
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            logger.LogInformation("User created a new account with password");

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page("/Account/ConfirmEmail", null, new { userId = user.Id, code, returnUrl }, Request.Scheme);

            var confirmModel = new EmailConfirmationEmailModel { ConfirmUrl = callbackUrl! };
            var confirmBody = await emailTemplateService.RenderAsync("EmailConfirmation", confirmModel);
            await emailSender.SendEmailAsync(Input.Email, "Confirm your email", confirmBody);

            if (userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
            }

            await signInManager.SignInAsync(user, false);
            return LocalRedirect(returnUrl);
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
