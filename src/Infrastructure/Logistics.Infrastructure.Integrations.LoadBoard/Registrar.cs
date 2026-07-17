using Logistics.Infrastructure.Integrations.LoadBoard.Credit;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.OneTwo3;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Truckstop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

public static class Registrar
{
    /// <summary>
    ///     Add LoadBoard provider integrations (DAT, Truckstop, 123Loadboard, Demo).
    /// </summary>
    public static IServiceCollection AddLoadBoardIntegrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configuration
        services.Configure<LoadBoardOptions>(
            configuration.GetSection(LoadBoardOptions.SectionName));

        // LoadBoard providers (with HttpClient for external APIs)
        services.AddHttpClient<DatLoadBoardService>();
        services.AddHttpClient<TruckstopLoadBoardService>();
        services.AddHttpClient<OneTwo3LoadBoardService>();
        services.AddScoped<DemoLoadBoardService>();

        // Factory pattern for provider selection
        services.AddScoped<ILoadBoardProviderFactory, LoadBoardProviderFactory>();

        // Broker credit check (cache -> demo/FMCSA authority fallback)
        services.Configure<FmcsaOptions>(configuration.GetSection(FmcsaOptions.SectionName));
        services.AddHttpClient<FmcsaClient>();
        services.AddScoped<IBrokerCreditService, BrokerCreditService>();

        return services;
    }
}
