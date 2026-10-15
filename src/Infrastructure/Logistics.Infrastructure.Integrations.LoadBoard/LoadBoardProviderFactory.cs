using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers;
using Logistics.Infrastructure.Integrations.LoadBoard.Providers.Dat;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

internal sealed class LoadBoardProviderFactory(
    IServiceProvider serviceProvider,
    ILogger<LoadBoardProviderFactory> logger)
    : ProviderFactoryBase<ILoadBoardProviderService, LoadBoardProviderType>(serviceProvider, logger),
        ILoadBoardProviderFactory
{
    protected override string FamilyName => "Load board";

    protected override IReadOnlyDictionary<LoadBoardProviderType, Type> Providers => ProviderMap;

    private static readonly IReadOnlyDictionary<LoadBoardProviderType, Type> ProviderMap =
        new Dictionary<LoadBoardProviderType, Type>
        {
            [LoadBoardProviderType.Dat] = typeof(DatLoadBoardService),
            [LoadBoardProviderType.Demo] = typeof(DemoLoadBoardService)
        };

    public ILoadBoardProviderService GetProvider(LoadBoardConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }
}
