using Logistics.Application.Abstractions.FuelCards;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.FuelCards.Providers;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.FuelCards;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Add fuel card provider integrations (WEX, EFS, Demo).
        /// </summary>
        public IServiceCollection AddFuelCardIntegrations(
    IConfiguration configuration)
        {
            // Fuel card providers (with HttpClient for external APIs)
            services.AddHttpClient<WexFuelCardService>();
            services.AddHttpClient<EfsFuelCardService>();
            services.AddScoped<DemoFuelCardService>();

            // Options binding + factory pattern for provider selection
            services.AddProviderIntegration<FuelCardsOptions, IFuelCardProviderFactory, FuelCardProviderFactory>(
                configuration, FuelCardsOptions.SectionName);

            return services;
        }
    }
}
