using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

internal sealed class ConnectQuickBooksHandler(
    ITenantUnitOfWork tenantUow,
    IAccountingProviderFactory providerFactory,
    IOAuthStateProtector stateProtector,
    ILogger<ConnectQuickBooksHandler> logger)
    : IRequestHandler<ConnectQuickBooksCommand, Result>
{
    public async Task<Result> Handle(ConnectQuickBooksCommand req, CancellationToken ct)
    {
        var tenantId = stateProtector.TryUnprotect(req.State);
        if (tenantId is null)
        {
            logger.LogWarning("QuickBooks callback received an invalid or tampered state");
            return Result.Fail("Invalid OAuth state.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(tenantId.Value);

        var provider = providerFactory.GetProvider(AccountingProviderType.QuickBooksOnline);
        var connection = await provider.ExchangeCodeAsync(req.Code, req.RealmId, ct);

        var repo = tenantUow.Repository<AccountingProviderConfiguration>();
        var config = await repo.GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline, ct);

        if (config is null)
        {
            config = new AccountingProviderConfiguration { ProviderType = AccountingProviderType.QuickBooksOnline };
            await repo.AddAsync(config, ct);
        }

        config.RealmId = connection.RealmId;
        config.CompanyName = connection.CompanyName;
        config.AccessToken = connection.AccessToken;
        config.RefreshToken = connection.RefreshToken;
        config.TokenExpiresAt = connection.AccessTokenExpiresAt;
        config.RefreshTokenExpiresAt = connection.RefreshTokenExpiresAt;
        config.IsActive = true;

        await ResolveDefaultAccountsAsync(provider, config, ct);

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation("Connected QuickBooks Online for tenant {TenantId} (realm {RealmId})",
            tenantId, connection.RealmId);
        return Result.Ok();
    }

    /// <summary>
    ///     Pick sensible default QBO accounts (a bank/asset account for payments and an expense
    ///     account for expense lines) from the company's Chart of Accounts, so expense pushes
    ///     have the required AccountRefs. Best-effort - leaves them null if none can be resolved.
    /// </summary>
    private async Task ResolveDefaultAccountsAsync(
        IAccountingProviderService provider, AccountingProviderConfiguration config, CancellationToken ct)
    {
        try
        {
            // Re-initialize with the just-persisted tokens so account queries authenticate.
            provider.Initialize(config);
            var accounts = await provider.GetAccountsAsync(ct);

            config.DefaultPaymentAccountId ??=
                accounts.FirstOrDefault(a => a.AccountType == "Bank")?.Id
                ?? accounts.FirstOrDefault(a => a.Classification == "Asset")?.Id;

            config.DefaultExpenseAccountId ??=
                accounts.FirstOrDefault(a => a.AccountType == "Expense")?.Id
                ?? accounts.FirstOrDefault(a => a.Classification == "Expense")?.Id;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not resolve default QBO accounts; expense sync will be skipped until set");
        }
    }
}
