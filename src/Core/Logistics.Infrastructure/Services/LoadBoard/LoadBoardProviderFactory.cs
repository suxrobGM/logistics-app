using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Services.Dat;
using Logistics.Infrastructure.Services.OneTwo3;
using Logistics.Infrastructure.Services.Truckstop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Services;

internal class LoadBoardProviderFactory(
    IServiceProvider serviceProvider,
    ILogger<LoadBoardProviderFactory> logger)
    : ILoadBoardProviderFactory
{
    public ILoadBoardProviderService GetProvider(LoadBoardProviderType providerType)
    {
        ILoadBoardProviderService service = providerType switch
        {
            LoadBoardProviderType.Dat => serviceProvider.GetRequiredService<DatLoadBoardService>(),
            LoadBoardProviderType.Truckstop => serviceProvider.GetRequiredService<TruckstopLoadBoardService>(),
            LoadBoardProviderType.OneTwo3Loadboard => serviceProvider.GetRequiredService<OneTwo3LoadBoardService>(),
            LoadBoardProviderType.Demo => serviceProvider.GetRequiredService<DemoLoadBoardService>(),
            _ => throw new NotSupportedException($"Load board provider '{providerType}' is not supported.")
        };

        logger.LogDebug("Created load board provider service for {ProviderType}", providerType);
        return service;
    }

    public ILoadBoardProviderService GetProvider(LoadBoardConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }

    public bool IsProviderSupported(LoadBoardProviderType providerType)
    {
        return providerType is LoadBoardProviderType.Dat
            or LoadBoardProviderType.Truckstop
            or LoadBoardProviderType.OneTwo3Loadboard
            or LoadBoardProviderType.Demo;
    }
}
