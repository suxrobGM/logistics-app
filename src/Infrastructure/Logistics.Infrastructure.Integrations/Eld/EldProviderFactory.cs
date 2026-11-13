using Logistics.Application.Abstractions.Integrations;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;

namespace Logistics.Infrastructure.Integrations.Eld;

internal sealed class EldProviderFactory(IServiceProvider serviceProvider) : IEldProviderFactory
{
    public IEldProviderService GetProvider(EldProviderType providerType) =>
        serviceProvider.GetRequiredProvider<IEldProviderService>(providerType, "ELD");

    public IEldProviderService GetProvider(EldProviderConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }

    public bool IsProviderSupported(EldProviderType providerType) =>
        serviceProvider.HasProvider<IEldProviderService>(providerType);
}
