using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.Eld.Providers;
using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Logistics.Infrastructure.Integrations.Eld.Providers.Motive;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.Eld;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.Integrations.Eld;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Add ELD provider integrations (Samsara, Motive, TT ELD, Demo).
        /// </summary>
        public IServiceCollection AddEldIntegrations(
            IConfiguration configuration)
        {
            services.AddHttpClient<SamsaraEldService>();
            services.AddHttpClient<MotiveEldService>();
            services.AddHttpClient<TtEldService>();
            services.AddHttpClient<GeotabClient>();
            services.AddScoped<GeotabEldService>();
            services.AddScoped<DemoEldService>();

            services.AddKeyedProvider<IEldProviderService, SamsaraEldService>(EldProviderType.Samsara);
            services.AddKeyedProvider<IEldProviderService, MotiveEldService>(EldProviderType.Motive);
            services.AddKeyedProvider<IEldProviderService, TtEldService>(EldProviderType.TtEld);
            services.AddKeyedProvider<IEldProviderService, DemoEldService>(EldProviderType.Demo);
            services.AddKeyedProvider<IEldProviderService, GeotabEldService>(EldProviderType.Geotab);

            // Options binding + factory pattern for provider selection
            services.AddProviderIntegration<EldOptions, IEldProviderFactory, EldProviderFactory>(
                configuration, EldOptions.SectionName);
            return services;
        }
    }
}
