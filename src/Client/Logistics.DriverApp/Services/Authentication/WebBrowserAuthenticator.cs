﻿using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient.Browser;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace Logistics.DriverApp.Services.Authentication;

public class WebBrowserAuthenticator : IBrowser
{
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await WebAuthenticator.Default.AuthenticateAsync(new Uri(options.StartUrl), new Uri(options.EndUrl));

            var url = new RequestUrl(options.EndUrl)
                .Create(new Parameters(result.Properties));

            return new BrowserResult
            {
                Response = url,
                ResultType = BrowserResultType.Success
            };
        }
        catch (TaskCanceledException)
        {
            return new BrowserResult
            {
                ResultType = BrowserResultType.UserCancel,
                ErrorDescription = "Login canceled by the user"
            };
        }
    }
}
