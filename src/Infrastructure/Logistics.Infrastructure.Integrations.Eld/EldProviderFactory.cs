using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.Eld.Providers;
using Logistics.Infrastructure.Integrations.Eld.Providers.Geotab;
using Logistics.Infrastructure.Integrations.Eld.Providers.Motive;
using Logistics.Infrastructure.Integrations.Eld.Providers.Samsara;
using Logistics.Infrastructure.Integrations.Eld.Providers.TtEld;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Eld;

namespace Logistics.Infrastructure.Integrations.Eld;

internal sealed class EldProviderFactory(
    IServiceProvider serviceProvider,
    ILogger<EldProviderFactory> logger)
    : ProviderFactoryBase<IEldProviderService, EldProviderType>(serviceProvider, logger), IEldProviderFactory
{
    protected override string FamilyName => "ELD";

    protected override IReadOnlyDictionary<EldProviderType, Type> Providers => ProviderMap;

    private static readonly IReadOnlyDictionary<EldProviderType, Type> ProviderMap =
        new Dictionary<EldProviderType, Type>
        {
            [EldProviderType.Samsara] = typeof(SamsaraEldService),
            [EldProviderType.Motive] = typeof(MotiveEldService),
            [EldProviderType.TtEld] = typeof(TtEldService),
            [EldProviderType.Demo] = typeof(DemoEldService),
            [EldProviderType.Geotab] = typeof(GeotabEldService)
        };

    public IEldProviderService GetProvider(EldProviderConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }
}
