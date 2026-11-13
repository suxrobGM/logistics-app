using Logistics.Application.Abstractions.Integrations;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.Eld.Providers;
using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Logistics.Infrastructure.Integrations.Eld.Providers.Motive;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Eld;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEldIntegrations(
            IConfiguration configuration)
        {
            services.AddHttpClient<SamsaraEldService>();
            services.AddHttpClient<MotiveEldService>((sp, client) =>
            {
                var motive = sp.GetRequiredService<IOptions<EldOptions>>().Value.Motive;
                client.BaseAddress = ToBaseAddress(motive.BaseUrl);
            });
            services.AddHttpClient<TtEldService>((sp, client) =>
            {
                var ttEld = sp.GetRequiredService<IOptions<EldOptions>>().Value.TtEld;
                client.BaseAddress = ToBaseAddress(ttEld.BaseUrl);
            });
            services.AddHttpClient<GeotabClient>();
            services.AddScoped<GeotabEldService>();
            services.AddScoped<DemoEldService>();

            services.AddKeyedProvider<IEldProviderService, SamsaraEldService>(EldProviderType.Samsara);
            services.AddKeyedProvider<IEldProviderService, MotiveEldService>(EldProviderType.Motive);
            services.AddKeyedProvider<IEldProviderService, TtEldService>(EldProviderType.TtEld);
            services.AddKeyedProvider<IEldProviderService, DemoEldService>(EldProviderType.Demo);
            services.AddKeyedProvider<IEldProviderService, GeotabEldService>(EldProviderType.Geotab);

            services.AddProviderIntegration<EldOptions, IEldProviderFactory, EldProviderFactory>(
                configuration, EldOptions.SectionName);
            return services;
        }
    }

    /// <summary>The trailing slash keeps a path base such as Motive's <c>/v1</c> when resolving relative URLs.</summary>
    private static Uri ToBaseAddress(string baseUrl) => new($"{baseUrl.TrimEnd('/')}/");
}
