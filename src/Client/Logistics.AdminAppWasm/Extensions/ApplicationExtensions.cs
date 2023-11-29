using Logistics.AdminApp.Authorization;
using Logistics.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Logistics.AdminApp.Extensions;

internal static class ApplicationExtensions
{
    public static WebAssemblyHost ConfigureServices(this WebAssemblyHostBuilder builder)
    {
#if !DEBUG
        AddSecretsJson(builder.Configuration);
#endif
        builder.Services.AddWebApiClient(builder.Configuration);
        
        builder.Services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind("Oidc", options.ProviderOptions);
        });

        builder.Services.AddAuthorizationCore();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        return builder.Build();
    }

    private static void AddSecretsJson(IConfigurationBuilder configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.secrets.json");
        configuration.AddJsonFile(path, true);
    }
}
