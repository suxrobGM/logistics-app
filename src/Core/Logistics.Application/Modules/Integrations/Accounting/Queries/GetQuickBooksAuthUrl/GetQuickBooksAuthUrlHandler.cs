using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Queries;

internal sealed class GetQuickBooksAuthUrlHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    IAccountingProviderFactory providerFactory,
    IOAuthStateProtector stateProtector)
    : IRequestHandler<GetQuickBooksAuthUrlQuery, Result<AccountingAuthUrlDto>>
{
    public Task<Result<AccountingAuthUrlDto>> Handle(GetQuickBooksAuthUrlQuery req, CancellationToken ct)
    {
        if (currentUser.GetUserId() is not { } userId)
        {
            return Task.FromResult(Result<AccountingAuthUrlDto>.Fail("Sign in to connect QuickBooks."));
        }

        var state = stateProtector.Protect(tenantUow.GetCurrentTenant().Id, userId);
        var provider = providerFactory.GetProvider(AccountingProviderType.QuickBooksOnline);
        var url = provider.BuildAuthorizationUrl(state);

        return Task.FromResult(Result<AccountingAuthUrlDto>.Ok(new AccountingAuthUrlDto { Url = url }));
    }
}
