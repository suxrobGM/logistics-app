using Open.IdentityServer.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages;

public static class Extensions
{
    extension(HttpContext context)
    {
        /// <summary>
        /// Determines if the authentication scheme support signout.
        /// </summary>
        public async Task<bool> GetSchemeSupportsSignOutAsync(string scheme)
        {
            var provider = context.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            var handler = await provider.GetHandlerAsync(context, scheme);
            return (handler is IAuthenticationSignOutHandler);
        }
    }

    extension(AuthorizationRequest context)
    {
        /// <summary>
        /// Checks if the redirect URI is for a native client.
        /// </summary>
        public bool IsNativeClient()
        {
            return !context.RedirectUri.StartsWith("https", StringComparison.Ordinal)
                   && !context.RedirectUri.StartsWith("http", StringComparison.Ordinal);
        }
    }

    extension(PageModel page)
    {
        /// <summary>
        /// Renders a loading page that is used to redirect back to the redirectUri.
        /// </summary>
        public IActionResult LoadingPage(string redirectUri)
        {
            page.HttpContext.Response.StatusCode = 200;
            page.HttpContext.Response.Headers["Location"] = "";

            return page.RedirectToPage("/Redirect/Index", new { RedirectUri = redirectUri });
        }
    }
}
