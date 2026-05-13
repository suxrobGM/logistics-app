using Logistics.Infrastructure.Integrations.Eld.Providers;
using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Logistics.Infrastructure.Integrations.Eld.Providers.Motive;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Infrastructure.Integrations.Eld;

public static class Registrar
{
    /// <summary>
    ///     Add ELD provider integrations (Samsara, Motive, TT ELD, Demo).
    /// </summary>
    public static IServiceCollection AddEldIntegrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<EldOptions>(configuration.GetSection(EldOptions.SectionName));

        // ELD providers (with HttpClient for external APIs)
        services.AddHttpClient<SamsaraEldService>();
        services.AddHttpClient<MotiveEldService>();
        services.AddHttpClient<TtEldService>();
        services.AddHttpClient<GeotabClient>();
        services.AddScoped<GeotabEldService>();
        services.AddScoped<DemoEldService>();

        // Factory pattern for provider selection
        services.AddScoped<IEldProviderFactory, EldProviderFactory>();
        return services;
    }
}
