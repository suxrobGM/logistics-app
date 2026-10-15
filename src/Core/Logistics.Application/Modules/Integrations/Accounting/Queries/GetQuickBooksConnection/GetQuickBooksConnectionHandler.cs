using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

internal sealed class GetQuickBooksConnectionHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetQuickBooksConnectionQuery, Result<AccountingConnectionDto>>
{
    public async Task<Result<AccountingConnectionDto>> Handle(GetQuickBooksConnectionQuery req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<AccountingProviderConfiguration>()
            .GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline, ct);

        if (config is not { IsActive: true })
        {
            return Result<AccountingConnectionDto>.Ok(new AccountingConnectionDto
            {
                ProviderType = AccountingProviderType.QuickBooksOnline
            });
        }

        return Result<AccountingConnectionDto>.Ok(new AccountingConnectionDto
        {
            ProviderType = AccountingProviderType.QuickBooksOnline,
            IsConnected = !config.NeedsReconnect && config.RefreshToken is not null,
            NeedsReconnect = config.NeedsReconnect,
            CompanyName = config.CompanyName,
            RealmId = config.RealmId,
            LastSyncedAt = config.LastSyncedAt,
            Settings = new AccountingSettingsDto
            {
                DefaultItemId = config.DefaultItemId,
                DepositAccountId = config.DepositAccountId,
                PaymentAccountId = config.DefaultPaymentAccountId,
                ExpenseAccountId = config.DefaultExpenseAccountId,
                PayrollExpenseAccountId = config.PayrollExpenseAccountId,
                FactoringClearingAccountId = config.FactoringClearingAccountId,
                FactoringFeeAccountId = config.FactoringFeeAccountId,
                SyncFromDate = config.SyncFromDate,
                Mappings = config.GetMappings()
            }
        });
    }
}
