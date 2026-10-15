using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// Connection status and posting settings for a tenant's accounting provider, surfaced to the settings UI.
/// </summary>
public record AccountingConnectionDto
{
    public AccountingProviderType ProviderType { get; set; }
    public bool IsConnected { get; set; }

    /// <summary>The provider rejected the stored tokens; sync is paused until the user reconnects.</summary>
    public bool NeedsReconnect { get; set; }

    public string? CompanyName { get; set; }
    public string? RealmId { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public int FailedItemCount { get; set; }
    public AccountingSettingsDto? Settings { get; set; }
}

/// <summary>Which provider accounts and items the sync posts to.</summary>
public record AccountingSettingsDto
{
    public string? DefaultItemId { get; set; }
    public string? DepositAccountId { get; set; }
    public string? PaymentAccountId { get; set; }
    public string? ExpenseAccountId { get; set; }
    public string? PayrollExpenseAccountId { get; set; }
    public string? FactoringClearingAccountId { get; set; }
    public string? FactoringFeeAccountId { get; set; }

    /// <summary>Transactions dated before this are not pushed.</summary>
    public DateTime? SyncFromDate { get; set; }

    public AccountingMappings Mappings { get; set; } = new();
}

/// <summary>
/// The provider OAuth authorization URL the frontend should redirect the browser to.
/// </summary>
public record AccountingAuthUrlDto
{
    public required string Url { get; set; }
}

/// <summary>
/// Result of exchanging an OAuth authorization code for tokens, plus the connected company.
/// </summary>
public record AccountingConnectionResultDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public required string RealmId { get; set; }
    public string? CompanyName { get; set; }
}

/// <summary>The connected company's accounts and items, for the settings pickers.</summary>
public record AccountingLookupsDto
{
    public List<QboAccountDto> Accounts { get; set; } = [];
    public List<QboItemDto> Items { get; set; } = [];
}

/// <summary>A record that failed to push, with the provider's error.</summary>
public record AccountingSyncIssueDto
{
    public required Guid Id { get; set; }
    public required QboLocalEntityType EntityType { get; set; }
    public required Guid LocalId { get; set; }

    /// <summary>What the user knows the record as, e.g. "Invoice 1007" or the customer name.</summary>
    public required string Label { get; set; }

    public string? Error { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Where a push writes in the provider. Without <see cref="QboId"/> the push creates the record;
/// <see cref="RequestId"/> makes that create idempotent, so a retry after a lost response does not
/// create a duplicate.
/// </summary>
public record QboWriteTarget(string? QboId, string? SyncToken, string RequestId);

/// <summary>
/// Provider-neutral result of an upsert: the external ID and its new concurrency token.
/// </summary>
public record QboUpsertResult
{
    public required string QboId { get; set; }
    public string? SyncToken { get; set; }
}

/// <summary>
/// A Chart-of-Accounts entry from the provider.
/// </summary>
public record QboAccountDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    /// <summary>QBO AccountType, e.g. "Bank", "Credit Card", "Expense", "Cost of Goods Sold".</summary>
    public string? AccountType { get; set; }

    /// <summary>QBO Classification: Asset, Liability, Equity, Revenue or Expense.</summary>
    public string? Classification { get; set; }
}

/// <summary>A product/service item from the provider, for invoice lines.</summary>
public record QboItemDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }

    /// <summary>QBO item Type, e.g. "Service", "NonInventory", "Inventory".</summary>
    public string? Type { get; set; }
}

/// <summary>Address fields for a customer push (mirrors the domain Address value object).</summary>
public record QboAddressPayload
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
}

/// <summary>Provider-neutral customer push payload.</summary>
public record QboCustomerPayload
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public QboAddressPayload? BillingAddress { get; set; }
}

/// <summary>Provider-neutral vendor push payload (an employee paid through bills, or an expense payee).</summary>
public record QboVendorPayload
{
    public required string DisplayName { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

/// <summary>A single invoice line for a push.</summary>
public record QboInvoiceLinePayload
{
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public int Quantity { get; set; } = 1;

    /// <summary>Sales item the line posts to; null lets QBO use its default item.</summary>
    public string? ItemId { get; set; }
}

/// <summary>Provider-neutral invoice push payload.</summary>
public record QboInvoicePayload
{
    public required string QboCustomerId { get; set; }
    public string? DocNumber { get; set; }
    public required string CurrencyCode { get; set; }
    public DateTime TxnDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal TaxTotal { get; set; }
    public List<QboInvoiceLinePayload> Lines { get; set; } = [];
}

/// <summary>Provider-neutral payment push payload, linked to a pushed invoice.</summary>
public record QboPaymentPayload
{
    public required string QboCustomerId { get; set; }
    public required string QboInvoiceId { get; set; }
    public decimal Amount { get; set; }
    public required string CurrencyCode { get; set; }
    public DateTime TxnDate { get; set; }
    public string? ReferenceNumber { get; set; }

    /// <summary>Account to deposit to; null leaves the provider default (QBO: Undeposited Funds).</summary>
    public string? DepositAccountId { get; set; }
}

/// <summary>Provider-neutral expense (QBO Purchase) push payload.</summary>
public record QboExpensePayload
{
    public required string PaymentAccountId { get; set; }

    /// <summary>Provider account type of <see cref="PaymentAccountId"/> (QBO "Bank" or "Credit Card").</summary>
    public string? PaymentAccountType { get; set; }

    public required string ExpenseAccountId { get; set; }
    public decimal Amount { get; set; }
    public required string CurrencyCode { get; set; }
    public DateTime TxnDate { get; set; }
    public string? DocNumber { get; set; }
    public string? Description { get; set; }

    /// <summary>The payee, when the expense names a vendor.</summary>
    public string? VendorId { get; set; }
}

/// <summary>Provider-neutral bill payload: what the company owes a vendor (driver pay).</summary>
public record QboBillPayload
{
    public required string VendorId { get; set; }
    public required string ExpenseAccountId { get; set; }
    public decimal Amount { get; set; }
    public required string CurrencyCode { get; set; }
    public DateTime TxnDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? DocNumber { get; set; }
    public string? Description { get; set; }
}
