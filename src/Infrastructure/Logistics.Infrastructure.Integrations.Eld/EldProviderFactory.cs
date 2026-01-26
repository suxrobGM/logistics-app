using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Eld;

internal class EldProviderFactory(
    IServiceProvider serviceProvider,
    ILogger<EldProviderFactory> logger)
    : IEldProviderFactory
{
    public IEldProviderService GetProvider(EldProviderType providerType)
    {
        IEldProviderService service = providerType switch
        {
            EldProviderType.Samsara => serviceProvider.GetRequiredService<SamsaraEldService>(),
            EldProviderType.Motive => serviceProvider.GetRequiredService<MotiveEldService>(),
            EldProviderType.Demo => serviceProvider.GetRequiredService<DemoEldService>(),
            EldProviderType.Geotab => throw new NotImplementedException("Geotab ELD provider is not supported"),
            EldProviderType.Omnitracs => throw new NotImplementedException("Omnitracs ELD provider is not supported"),
            EldProviderType.PeopleNet => throw new NotImplementedException("PeopleNet ELD provider is not supported"),
            _ => throw new NotSupportedException($"ELD provider '{providerType}' is not supported")
        };

        logger.LogDebug("Created ELD provider service for {ProviderType}", providerType);
        return service;
    }

    public IEldProviderService GetProvider(EldProviderConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }

    public bool IsProviderSupported(EldProviderType providerType)
    {
        return providerType is EldProviderType.Samsara or EldProviderType.Motive or EldProviderType.Demo;
    }
}
