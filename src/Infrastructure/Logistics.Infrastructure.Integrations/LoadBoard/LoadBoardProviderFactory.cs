using Logistics.Application.Abstractions.Integrations;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;

namespace Logistics.Infrastructure.Integrations.LoadBoard;

internal sealed class LoadBoardProviderFactory(IServiceProvider serviceProvider) : ILoadBoardProviderFactory
{
    public ILoadBoardProviderService GetProvider(LoadBoardProviderType providerType) =>
        serviceProvider.GetRequiredProvider<ILoadBoardProviderService>(providerType, "Load board");

    public bool IsProviderSupported(LoadBoardProviderType providerType) =>
        serviceProvider.HasProvider<ILoadBoardProviderService>(providerType);
}
