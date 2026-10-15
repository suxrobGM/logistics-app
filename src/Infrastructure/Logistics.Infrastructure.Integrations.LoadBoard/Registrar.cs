using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.LoadBoard.Credit;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

public static class Registrar
{
    /// <summary>
    ///     Add LoadBoard provider integrations (DAT, Demo).
    /// </summary>
    public static IServiceCollection AddLoadBoardIntegrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<DatLoadBoardService>();
        services.AddScoped<DemoLoadBoardService>();

        services.AddProviderIntegration<LoadBoardOptions, ILoadBoardProviderFactory, LoadBoardProviderFactory>(
            configuration, LoadBoardOptions.SectionName);

        // Broker credit check (cache -> demo/FMCSA authority fallback)
        services.Configure<FmcsaOptions>(configuration.GetSection(FmcsaOptions.SectionName));
        services.AddHttpClient<FmcsaClient>();
        services.AddScoped<IBrokerCreditService, BrokerCreditService>();

        return services;
    }
}
