using IdentityModel.OidcClient;
using Logistics.DriverApp.Services;
using Logistics.DriverApp.Services.Authentication;
using Logistics.DriverApp.Services.LocationTracking;
using Microsoft.Extensions.Configuration;
using Syncfusion.Licensing;

#if ANDROID
using Logistics.DriverApp.Platforms.Android.Services;
#elif IOS
using Logistics.DriverApp.Platforms.iOS.Services;
#endif

namespace Logistics.DriverApp.Extensions;

public static class ApplicationExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.BuildConfiguration();
        SyncfusionLicenseProvider.RegisterLicense(configuration.GetValue<string>("SyncfusionKey"));
        
        var oidcOptions = configuration.GetSection("OidcClient").Get<OidcClientOptions>() 
                          ?? throw new NullReferenceException("Could not get OidcClient form the appsettings.json file");
        
        services.AddSingleton(oidcOptions);
        services.AddWebApiClient(configuration);
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<ITenantService, TenantService>();
        services.AddSingleton<IMapsService, GoogleMapsService>();
        services.AddScoped<AppShellViewModel>();
        services.AddScoped<ActiveLoadsPageViewModel>();
        services.AddScoped<AccountPageViewModel>();
        services.AddScoped<LoginPageViewModel>();
        services.AddScoped<ChangeOrganizationPageViewModel>();
        services.AddScoped<IdentityModel.OidcClient.Browser.IBrowser, WebBrowserAuthenticator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILocationTracker, LocationTracker>();
        services.AddScoped<ILocationTrackerBackgroundService, LocationTrackerBackgroundService>();
        return builder;
    }

    private static IConfiguration BuildConfiguration(this MauiAppBuilder builder)
    {
        var configuration = builder.Configuration
            .AddJsonConfig("appsettings.json")
#if !DEBUG
            .AddJsonConfig("appsettings.secrets.json")
#endif
            .Build();

        return configuration;
    }
}
