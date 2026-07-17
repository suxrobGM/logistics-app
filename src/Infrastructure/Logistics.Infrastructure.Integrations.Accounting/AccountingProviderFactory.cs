using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Accounting.Providers;
using Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;
using Logistics.Infrastructure.Integrations.Common;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Accounting;

internal sealed class AccountingProviderFactory(
    IServiceProvider serviceProvider,
    ILogger<AccountingProviderFactory> logger)
    : ProviderFactoryBase<IAccountingProviderService, AccountingProviderType>(serviceProvider, logger),
        IAccountingProviderFactory
{
    protected override string FamilyName => "Accounting";

    protected override IReadOnlyDictionary<AccountingProviderType, Type> Providers => ProviderMap;

    private static readonly IReadOnlyDictionary<AccountingProviderType, Type> ProviderMap =
        new Dictionary<AccountingProviderType, Type>
        {
            [AccountingProviderType.QuickBooksOnline] = typeof(QuickBooksOnlineService),
            [AccountingProviderType.Demo] = typeof(DemoAccountingService)
        };

    public IAccountingProviderService GetProvider(AccountingProviderConfiguration configuration)
    {
        var service = GetProvider(configuration.ProviderType);
        service.Initialize(configuration);
        return service;
    }
}
