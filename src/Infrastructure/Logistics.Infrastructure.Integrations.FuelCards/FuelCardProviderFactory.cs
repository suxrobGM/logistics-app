using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.FuelCards.Providers;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Efs;
using Logistics.Infrastructure.Integrations.FuelCards.Providers.Wex;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.FuelCards;

internal class FuelCardProviderFactory(
    IServiceProvider serviceProvider,
    ILogger<FuelCardProviderFactory> logger)
    : IFuelCardProviderFactory
{
    public IFuelCardProviderService GetProvider(FuelCardProviderType providerType)
    {
        IFuelCardProviderService service = providerType switch
        {
            FuelCardProviderType.Wex => serviceProvider.GetRequiredService<WexFuelCardService>(),
            FuelCardProviderType.Efs => serviceProvider.GetRequiredService<EfsFuelCardService>(),
            FuelCardProviderType.Demo => serviceProvider.GetRequiredService<DemoFuelCardService>(),
            FuelCardProviderType.Comdata => throw new NotImplementedException("Comdata fuel card provider is not supported yet"),
            _ => throw new NotSupportedException($"Fuel card provider '{providerType}' is not supported")
        };

        logger.LogDebug("Created fuel card provider service for {ProviderType}", providerType);
        return service;
    }

    public IFuelCardProviderService GetProvider(FuelCardProviderConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }

    public bool IsProviderSupported(FuelCardProviderType providerType)
    {
        return providerType is FuelCardProviderType.Wex or
            FuelCardProviderType.Efs or
            FuelCardProviderType.Demo;
    }
}
