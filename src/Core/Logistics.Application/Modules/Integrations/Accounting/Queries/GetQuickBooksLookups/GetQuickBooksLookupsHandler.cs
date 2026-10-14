using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

internal sealed class GetQuickBooksLookupsHandler(
    ITenantUnitOfWork tenantUow,
    IAccountingConnectionService connectionService)
    : IRequestHandler<GetQuickBooksLookupsQuery, Result<AccountingLookupsDto>>
{
    public async Task<Result<AccountingLookupsDto>> Handle(GetQuickBooksLookupsQuery req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<AccountingProviderConfiguration>()
            .GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline && c.IsActive, ct);
        if (config is null)
        {
            return Result<AccountingLookupsDto>.Fail("QuickBooks is not connected.");
        }

        var provider = await connectionService.GetProviderAsync(config, ct);
        if (provider is null)
        {
            return Result<AccountingLookupsDto>.Fail("QuickBooks needs to be reconnected.");
        }

        try
        {
            var accounts = await provider.GetAccountsAsync(ct);
            var items = await provider.GetItemsAsync(ct);
            return Result<AccountingLookupsDto>.Ok(new AccountingLookupsDto
            {
                Accounts = accounts.ToList(),
                Items = items.ToList()
            });
        }
        catch (AccountingReconnectRequiredException)
        {
            await connectionService.RejectAccessTokenAsync(config, ct);
            return Result<AccountingLookupsDto>.Fail("QuickBooks rejected the connection. Try again in a moment, or reconnect.");
        }
    }
}
