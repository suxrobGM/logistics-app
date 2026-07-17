using Logistics.Application.Abstractions.FuelCards;
using Logistics.Infrastructure.Integrations.FuelCards.Providers;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.FuelCards;

public static class Registrar
{
    /// <summary>
    ///     Add fuel card provider integrations (WEX, EFS, Demo).
    /// </summary>
    public static IServiceCollection AddFuelCardIntegrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<FuelCardsOptions>(configuration.GetSection(FuelCardsOptions.SectionName));

        // Fuel card providers (with HttpClient for external APIs)
        services.AddHttpClient<WexFuelCardService>();
        services.AddHttpClient<EfsFuelCardService>();
        services.AddScoped<DemoFuelCardService>();

        // Factory pattern for provider selection
        services.AddScoped<IFuelCardProviderFactory, FuelCardProviderFactory>();

        return services;
    }
}
