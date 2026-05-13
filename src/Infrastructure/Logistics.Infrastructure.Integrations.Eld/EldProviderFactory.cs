using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Eld.Providers;
using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Logistics.Infrastructure.Integrations.Eld.Providers.Motive;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Eld;

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
            EldProviderType.TtEld => serviceProvider.GetRequiredService<TtEldService>(),
            EldProviderType.Demo => serviceProvider.GetRequiredService<DemoEldService>(),
            EldProviderType.Geotab => serviceProvider.GetRequiredService<GeotabEldService>(),
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
        return providerType is EldProviderType.Samsara or
            EldProviderType.Motive or
            EldProviderType.TtEld or
            EldProviderType.Geotab or
            EldProviderType.Demo;
    }
}
