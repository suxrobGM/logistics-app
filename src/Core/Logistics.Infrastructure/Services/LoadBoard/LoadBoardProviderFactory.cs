using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
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
        // Note: DAT, Truckstop, and 123Loadboard providers are defined but not yet implemented.
        // Only Demo provider is available for testing.
        ILoadBoardProviderService service = providerType switch
        {
            LoadBoardProviderType.Demo => serviceProvider.GetRequiredService<DemoLoadBoardService>(),
            _ => throw new NotSupportedException($"Load board provider '{providerType}' is not yet implemented. Only Demo provider is currently available.")
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
        // Only Demo is currently implemented
        return providerType is LoadBoardProviderType.Demo;
    }
}
