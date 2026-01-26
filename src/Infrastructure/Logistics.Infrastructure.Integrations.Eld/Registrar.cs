using Logistics.Application.Services;
using Logistics.Infrastructure.Integrations.Eld.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.Eld;

public static class Registrar
{
    /// <summary>
    ///     Add ELD provider integrations (Samsara, Motive, Demo).
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
        services.AddScoped<DemoEldService>();

        // Factory pattern for provider selection
        services.AddScoped<IEldProviderFactory, EldProviderFactory>();
        return services;
    }
}
