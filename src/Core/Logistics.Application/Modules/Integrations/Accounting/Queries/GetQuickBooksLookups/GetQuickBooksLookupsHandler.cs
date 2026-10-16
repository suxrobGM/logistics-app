using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

internal sealed class GetQuickBooksLookupsHandler(IAccountingConnectionService connectionService)
    : IRequestHandler<GetQuickBooksLookupsQuery, Result<AccountingLookupsDto>>
{
    public Task<Result<AccountingLookupsDto>> Handle(GetQuickBooksLookupsQuery req, CancellationToken ct) =>
        connectionService.ExecuteAsync(async (provider, _) =>
        {
            var accountsTask = provider.GetAccountsAsync(ct);
            var itemsTask = provider.GetItemsAsync(ct);
            await Task.WhenAll(accountsTask, itemsTask);
            return new AccountingLookupsDto
            {
                Accounts = (await accountsTask).ToList(),
                Items = (await itemsTask).ToList()
            };
        }, ct);
}
