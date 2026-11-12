using Logistics.Infrastructure.Integrations.Accounting;
using Logistics.Infrastructure.Integrations.Eld;
using Logistics.Infrastructure.Integrations.FuelCards;
using Logistics.Infrastructure.Integrations.LoadBoard;
using Logistics.Infrastructure.Licensing;
using Logistics.Infrastructure.Vin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers every integration family. A host that needs only some of them calls the
        ///     family registrar instead, e.g. <c>AddFuelCardIntegrations</c>.
        /// </summary>
        public IServiceCollection AddIntegrationsInfrastructure(IConfiguration configuration)
        {
            services.AddVinInfrastructure();
            services.AddLicensingInfrastructure();
            services.AddEldIntegrations(configuration);
            services.AddLoadBoardIntegrations(configuration);
            services.AddAccountingIntegrations(configuration);
            services.AddFuelCardIntegrations(configuration);
            return services;
        }
    }
}
