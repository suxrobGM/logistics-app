using Logistics.Infrastructure.Integrations.Common;
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
        services.AddHttpClient<SamsaraEldService>().AddHttpMessageHandler(() => new RateLimitRetryHandler());
        services.AddHttpClient<MotiveEldService>().AddHttpMessageHandler(() => new RateLimitRetryHandler());
        services.AddHttpClient<TtEldService>().AddHttpMessageHandler(() => new RateLimitRetryHandler());
        services.AddHttpClient<GeotabClient>().AddHttpMessageHandler(() => new RateLimitRetryHandler());
        services.AddScoped<GeotabEldService>();
        services.AddScoped<DemoEldService>();

        // Options binding + factory pattern for provider selection
        services.AddProviderIntegration<EldOptions, IEldProviderFactory, EldProviderFactory>(
            configuration, EldOptions.SectionName);
        return services;
    }
}
