using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Services;

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
        return providerType is EldProviderType.Samsara or EldProviderType.Motive;
    }
}
