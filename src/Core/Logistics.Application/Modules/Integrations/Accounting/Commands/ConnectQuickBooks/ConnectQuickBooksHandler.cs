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
        var state = stateProtector.TryUnprotect(req.State);
        if (state is null)
        {
            logger.LogWarning("QuickBooks callback received an invalid, tampered or expired state");
            return Result.Fail("The QuickBooks connection link expired. Start the connection again.");
        }

        await tenantUow.SetCurrentTenantByIdAsync(state.TenantId);

        var provider = providerFactory.GetProvider(AccountingProviderType.QuickBooksOnline);
        AccountingConnectionResultDto connection;
        try
        {
            connection = await provider.ExchangeCodeAsync(req.Code, req.RealmId, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "QuickBooks code exchange failed for tenant {TenantId}", state.TenantId);
            return Result.Fail("QuickBooks did not accept the connection. Try connecting again.");
        }

        var repo = tenantUow.Repository<AccountingProviderConfiguration>();
        var config = await repo.GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline, ct);
        if (config is null)
        {
            config = new AccountingProviderConfiguration { ProviderType = AccountingProviderType.QuickBooksOnline };
            await repo.AddAsync(config, ct);
        }

        var companyChanged = config.Connect(connection.RealmId, connection.CompanyName, connection.AccessToken,
            connection.RefreshToken, connection.AccessTokenExpiresAt, connection.RefreshTokenExpiresAt);

        if (companyChanged)
        {
            // Every stored QBO id belongs to the previous company; pushing with them would hit the wrong records.
            var mappings = tenantUow.Repository<QboEntityMapping>();
            foreach (var mapping in await mappings.GetListAsync(_ => true, ct))
            {
                mappings.Delete(mapping);
            }
        }

        provider.Initialize(config);
        await ResolveDefaultsAsync(provider, config, ct);
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "User {UserId} connected QuickBooks Online for tenant {TenantId} (realm {RealmId}, company changed: {CompanyChanged})",
            state.UserId, state.TenantId, connection.RealmId, companyChanged);
        return Result.Ok();
    }

    /// <summary>
    ///     Pick sensible defaults for anything not chosen yet - a bank account to pay expenses from, an
    ///     expense account, and the sales item invoice lines post to - so the first sync works before the
    ///     user opens the settings. Best-effort: leaves them null if the lists cannot be read.
    /// </summary>
    private async Task ResolveDefaultsAsync(
        IAccountingProviderService provider, AccountingProviderConfiguration config, CancellationToken ct)
    {
        try
        {
            var accounts = await provider.GetAccountsAsync(ct);
            if (config.DefaultPaymentAccountId is null &&
                accounts.FirstOrDefault(a => a.AccountType == "Bank") is { } bank)
            {
                config.DefaultPaymentAccountId = bank.Id;
                config.DefaultPaymentAccountType = bank.AccountType;
            }

            config.DefaultExpenseAccountId ??= accounts.FirstOrDefault(a => a.AccountType == "Expense")?.Id;

            var items = await provider.GetItemsAsync(ct);
            var services = items.Where(i => i.Type == "Service").ToList();
            config.DefaultItemId ??=
                services.FirstOrDefault(i => i.Name.Equals("Services", StringComparison.OrdinalIgnoreCase))?.Id
                ?? services.FirstOrDefault()?.Id;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not resolve default QBO accounts and items; the user can pick them in settings");
        }
    }
}
