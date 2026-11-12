using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.LoadBoard.Credit;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLoadBoardIntegrations(
            IConfiguration configuration)
        {
            services.AddHttpClient<DatLoadBoardService>();
            services.AddScoped<DemoLoadBoardService>();

            services.AddKeyedProvider<ILoadBoardProviderService, DatLoadBoardService>(LoadBoardProviderType.Dat);
            services.AddKeyedProvider<ILoadBoardProviderService, DemoLoadBoardService>(LoadBoardProviderType.Demo);

            services.AddProviderIntegration<LoadBoardOptions, ILoadBoardProviderFactory, LoadBoardProviderFactory>(
                configuration, LoadBoardOptions.SectionName);

            services.Configure<FmcsaOptions>(configuration.GetSection(FmcsaOptions.SectionName));
            services.AddHttpClient<FmcsaClient>();
            services.AddScoped<IBrokerCreditService, BrokerCreditService>();

            return services;
        }
    }
}
