using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Primitives.Enums;
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
        public IServiceCollection AddFuelCardIntegrations(
            IConfiguration configuration)
        {
            services.AddHttpClient<WexFuelCardService>();
            services.AddHttpClient<EfsFuelCardService>();
            services.AddScoped<DemoFuelCardService>();

            services.AddKeyedProvider<IFuelCardProviderService, WexFuelCardService>(FuelCardProviderType.Wex);
            services.AddKeyedProvider<IFuelCardProviderService, EfsFuelCardService>(FuelCardProviderType.Efs);
            services.AddKeyedProvider<IFuelCardProviderService, DemoFuelCardService>(FuelCardProviderType.Demo);

            services.AddProviderIntegration<FuelCardsOptions, IFuelCardProviderFactory, FuelCardProviderFactory>(
                configuration, FuelCardsOptions.SectionName);

            return services;
        }
    }
}
