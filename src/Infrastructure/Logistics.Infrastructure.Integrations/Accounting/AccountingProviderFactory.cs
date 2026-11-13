using Logistics.Application.Abstractions.Integrations;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Common;

namespace Logistics.Infrastructure.Integrations.Accounting;

internal sealed class AccountingProviderFactory(IServiceProvider serviceProvider) : IAccountingProviderFactory
{
    public IAccountingProviderService GetProvider(AccountingProviderType providerType) =>
        serviceProvider.GetRequiredProvider<IAccountingProviderService>(providerType, "Accounting");

    public IAccountingProviderService GetProvider(AccountingProviderConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }

    public bool IsProviderSupported(AccountingProviderType providerType) =>
        serviceProvider.HasProvider<IAccountingProviderService>(providerType);
}
