using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;

namespace Logistics.Infrastructure.Integrations.FuelCards;

internal sealed class FuelCardProviderFactory(IServiceProvider serviceProvider) : IFuelCardProviderFactory
{
    public IFuelCardProviderService GetProvider(FuelCardProviderType providerType) =>
        serviceProvider.GetRequiredProvider<IFuelCardProviderService>(providerType, "Fuel card");

    public IFuelCardProviderService GetProvider(FuelCardProviderConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }

    public bool IsProviderSupported(FuelCardProviderType providerType) =>
        serviceProvider.HasProvider<IFuelCardProviderService>(providerType);
}
