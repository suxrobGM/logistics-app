using Duende.IdentityModel.OidcClient;
using Logistics.DriverApp.Extensions;
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

namespace Logistics.DriverApp;

public static class Setup
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.BuildConfiguration();
        SyncfusionLicenseProvider.RegisterLicense(configuration.GetValue<string>("SyncfusionKey"));
        
        var oidcOptions = configuration.GetSection("Oidc").Get<OidcClientOptions>() 
                          ?? throw new NullReferenceException("Could not get Oidc form the appsettings.json file");
        
        services.AddSingleton(oidcOptions);
        services.AddApiHttpClient(configuration);
        services.AddSingleton<ITokenStorage, TokenStorage>();
        services.AddSingleton<ITenantService, TenantService>();
        services.AddSingleton<IMapsService, GoogleMapsService>();
        services.AddSingleton<ICache, InMemoryCache>();
        
        services.AddScoped<AppShellViewModel>();
        services.AddScoped<DashboardPageViewModel>();
        services.AddScoped<AccountPageViewModel>();
        services.AddScoped<LoginPageViewModel>();
        services.AddScoped<StatsPageViewModel>();
        services.AddScoped<LoadPageViewModel>();
        services.AddScoped<PastLoadsPageViewModel>();
        
        services.AddScoped<Duende.IdentityModel.OidcClient.Browser.IBrowser, WebBrowserAuthenticator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILoadProximityUpdater, LoadProximityUpdater>();
        services.AddScoped<ILocationTracker, LocationTracker>();
        services.AddScoped<ILocationTrackerBackgroundService, LocationTrackerBackgroundService>();
        RegisterRoutes();
        return builder;
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(LoadPage), typeof(LoadPage));
    }

    private static IConfiguration BuildConfiguration(this MauiAppBuilder builder)
    {
        var configuration = builder.Configuration
            .AddJsonConfig("appsettings.json")
            .Build();

        return configuration;
    }
}
