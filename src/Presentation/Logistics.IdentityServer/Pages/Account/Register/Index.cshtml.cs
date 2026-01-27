using System.Text;
using System.Text.Encodings.Web;
using Logistics.Application.Contracts.Services.Email;
using Logistics.Application.Services;
using Logistics.Domain.Entities;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Logistics.IdentityServer.Pages.Account.Register;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ICaptchaService _captchaService;
    private readonly ILogger<Index> _logger;

    public Index(
        IConfiguration configuration,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IEmailSender emailSender,
        ICaptchaService captchaService,
        ILogger<Index> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
        _captchaService = captchaService;
        _logger = logger;
        CaptchaSiteKey = configuration.GetValue<string>("GoogleRecaptcha:SiteKey");
    }

    [BindProperty]
    public InputModel Input { get; set; }
    public string ReturnUrl { get; set; }
    public string CaptchaSiteKey { get; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var captchaValue = HttpContext.Request.Form["g-recaptcha-response"].ToString();
        var validCaptcha = await _captchaService.VerifyCaptchaAsync(captchaValue);
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page("/Account/ConfirmEmail", null, new { userId = user.Id, code, returnUrl }, Request.Scheme);

            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
            }

            await _signInManager.SignInAsync(user, false);
            return LocalRedirect(returnUrl);
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
