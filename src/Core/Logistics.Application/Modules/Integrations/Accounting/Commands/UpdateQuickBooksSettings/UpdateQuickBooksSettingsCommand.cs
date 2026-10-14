using Logistics.Application.Abstractions.Common;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

/// <summary>
///     Choose which QuickBooks accounts and items the sync posts to. Every id is checked against the
///     connected company's lists. Applies to records pushed from now on.
/// </summary>
[RequiresFeature(TenantFeature.Accounting)]
public class UpdateQuickBooksSettingsCommand : ICommand
{
    /// <summary>Sales item for invoice lines whose type has no mapping.</summary>
    public string? DefaultItemId { get; set; }

    /// <summary>Account customer payments are deposited to; null uses Undeposited Funds.</summary>
    public string? DepositAccountId { get; set; }

    /// <summary>Bank or credit card account expenses are paid from. Expense sync needs it.</summary>
    public string? PaymentAccountId { get; set; }

    /// <summary>Expense account for categories with no mapping. Expense sync needs it.</summary>
    public string? ExpenseAccountId { get; set; }

    /// <summary>Expense account driver pay bills post to; null turns driver pay sync off.</summary>
    public string? PayrollExpenseAccountId { get; set; }

    /// <summary>Transactions dated before this are not pushed.</summary>
    public DateTime? SyncFromDate { get; set; }

    public AccountingMappings Mappings { get; set; } = new();
}
