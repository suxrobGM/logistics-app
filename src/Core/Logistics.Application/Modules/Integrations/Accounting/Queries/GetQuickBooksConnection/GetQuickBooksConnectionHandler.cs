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

        var dto = new AccountingConnectionDto
        {
            ProviderType = AccountingProviderType.QuickBooksOnline,
            IsConnected = config is { IsActive: true, AccessToken: not null },
            CompanyName = config?.CompanyName,
            RealmId = config?.RealmId,
            LastSyncedAt = config?.LastSyncedAt
        };

        return Result<AccountingConnectionDto>.Ok(dto);
    }
}
