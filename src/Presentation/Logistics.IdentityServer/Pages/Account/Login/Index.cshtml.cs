#nullable enable
using System.Text.RegularExpressions;

using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;

using Logistics.Domain.Entities;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Logistics.IdentityServer.Pages.Account.Login;

[SecurityHeaders]
[AllowAnonymous]
[EnableRateLimiting("login")]
public class Index(
    IIdentityServerInteractionService interaction,
    IAuthenticationSchemeProvider schemeProvider,
    IIdentityProviderStore identityProviderStore,
    IEventService events,
    UserManager<User> userManager,
    SignInManager<User> signInManager) : PageModel
{
    public ViewModel View { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = null!;

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        await BuildModelAsync(returnUrl);

        if (View.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return RedirectToPage("/ExternalLogin/Challenge", new { scheme = View.ExternalLoginScheme, returnUrl });
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // check if we are in the context of an authorization request
        var context = await interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

        // the user clicked the "cancel" button
        if (Input.Button != "login")
        {
            // if the user cancels, send a result back into IdentityServer as if they
            // denied the consent (even if this client does not require consent).
            // this will send back an access denied OIDC error response to the client.
            await interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

            if (context != null)
            {
                return context.IsNativeClient() ? this.LoadingPage(Input.ReturnUrl) : Redirect(Input.ReturnUrl);
            }
            return Redirect(Input.ReturnUrl);
        }

        User? user = null;
        const string emailPattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
        if (!Regex.IsMatch(Input.Email, emailPattern))
        {
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
        }
        else
        {
            user = await userManager.FindByEmailAsync(Input.Email);
        }

        if (user == null)
        {
            await events.RaiseAsync(new UserLoginFailureEvent(Input.Email, "invalid credentials", clientId: context?.Client.ClientId));
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
        }

        if (ModelState.IsValid)
        {
            var result = await signInManager.PasswordSignInAsync(user.UserName!, Input.Password, Input.RememberLogin, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                await events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId));

                if (context != null)
                {
                    return context.IsNativeClient() ? this.LoadingPage(Input.ReturnUrl) : Redirect(Input.ReturnUrl);
                }
                return Redirect(Input.ReturnUrl);
            }

            await events.RaiseAsync(new UserLoginFailureEvent(Input.Email, "invalid credentials", clientId: context?.Client.ClientId));
            ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
        }

        // something went wrong, show form with error
        await BuildModelAsync(Input.ReturnUrl);
        return Page();
    }

    private async Task BuildModelAsync(string? returnUrl)
    {
        Input = new InputModel
        {
            ReturnUrl = returnUrl ?? "/",
        };

        var context = await interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null && await schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

            // this is meant to short circuit the UI and only trigger the one external IdP
            View = new ViewModel
            {
                EnableLocalLogin = local,
            };

            Input.Email = context.LoginHint;

            if (!local)
            {
                View.ExternalProviders = [new ViewModel.ExternalProvider { AuthenticationScheme = context.IdP }];
            }

            return;
        }

        var schemes = await schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ViewModel.ExternalProvider
            {
                DisplayName = x.DisplayName ?? x.Name,
                AuthenticationScheme = x.Name
            }).ToList();

        var dynamicSchemes = (await identityProviderStore.GetAllSchemeNamesAsync())
            .Where(x => x.Enabled)
            .Select(x => new ViewModel.ExternalProvider
            {
                AuthenticationScheme = x.Scheme,
                DisplayName = x.DisplayName
            });
        providers.AddRange(dynamicSchemes);


        var allowLocal = true;
        var client = context?.Client;
        if (client != null)
        {
            allowLocal = client.EnableLocalLogin;
            if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Count != 0)
            {
                providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
            }
        }

        View = new ViewModel
        {
            AllowRememberLogin = LoginOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
            ExternalProviders = providers.ToArray()
        };
    }
}
