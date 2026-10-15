using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

internal sealed class UpdateQuickBooksSettingsHandler(
    ITenantUnitOfWork tenantUow,
    IAccountingConnectionService connectionService)
    : IRequestHandler<UpdateQuickBooksSettingsCommand, Result>
{
    private static readonly string[] PaymentAccountTypes = ["Bank", "Credit Card"];

    public async Task<Result> Handle(UpdateQuickBooksSettingsCommand req, CancellationToken ct)
    {
        var config = await tenantUow.Repository<AccountingProviderConfiguration>()
            .GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline && c.IsActive, ct);
        if (config is null)
        {
            return Result.Fail("QuickBooks is not connected.");
        }

        var provider = await connectionService.GetProviderAsync(config, ct);
        if (provider is null)
        {
            return Result.Fail("QuickBooks needs to be reconnected.");
        }

        var accounts = (await provider.GetAccountsAsync(ct)).ToDictionary(a => a.Id);
        var itemIds = (await provider.GetItemsAsync(ct)).Select(i => i.Id).ToHashSet();

        QboAccountDto? paymentAccount = null;
        if (req.PaymentAccountId is not null
            && (!accounts.TryGetValue(req.PaymentAccountId, out paymentAccount)
                || !PaymentAccountTypes.Contains(paymentAccount.AccountType)))
        {
            return Result.Fail("Expenses must be paid from a QuickBooks bank or credit card account.");
        }

        // The fee is a Purchase paid out of the clearing account, which QBO only allows from a bank account.
        if (req.FactoringClearingAccountId is not null
            && (!accounts.TryGetValue(req.FactoringClearingAccountId, out var clearing) || clearing.AccountType != "Bank"))
        {
            return Result.Fail("The factoring clearing account must be a QuickBooks bank-type account.");
        }

        if (req.FactoringClearingAccountId is not null && req.FactoringFeeAccountId is null)
        {
            return Result.Fail("Choose the expense account factoring fees post to.");
        }

        var accountIds = new[] { req.DepositAccountId, req.ExpenseAccountId, req.PayrollExpenseAccountId, req.FactoringFeeAccountId }
            .Concat(req.Mappings.CompanyExpenseAccounts.Select(m => m.QboId))
            .Concat(req.Mappings.TruckExpenseAccounts.Select(m => m.QboId))
            .OfType<string>();
        if (accountIds.Any(id => !accounts.ContainsKey(id)))
        {
            return Result.Fail("One of the chosen accounts is no longer in QuickBooks. Reload the page and pick again.");
        }

        var chosenItemIds = req.Mappings.Items.Select(m => m.QboId).Append(req.DefaultItemId).OfType<string>();
        if (chosenItemIds.Any(id => !itemIds.Contains(id)))
        {
            return Result.Fail("One of the chosen items is no longer in QuickBooks. Reload the page and pick again.");
        }

        config.DefaultItemId = req.DefaultItemId;
        config.DepositAccountId = req.DepositAccountId;
        config.DefaultPaymentAccountId = req.PaymentAccountId;
        config.DefaultPaymentAccountType = paymentAccount?.AccountType;
        config.DefaultExpenseAccountId = req.ExpenseAccountId;
        config.PayrollExpenseAccountId = req.PayrollExpenseAccountId;
        config.FactoringClearingAccountId = req.FactoringClearingAccountId;
        config.FactoringFeeAccountId = req.FactoringFeeAccountId;
        config.SyncFromDate = req.SyncFromDate is { } from ? DateTime.SpecifyKind(from.Date, DateTimeKind.Utc) : null;
        config.SetMappings(req.Mappings);

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
