using System.Text.Json.Serialization;

namespace Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;

// Wire models for the QBO v3 Accounting API (minor version pinned in QboEndpoints). Field names and
// shapes follow Intuit's entity reference: developer.intuit.com/app/developer/qbo/docs/api/accounting

// --- OAuth ---

internal sealed record QboTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = "";
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    [JsonPropertyName("x_refresh_token_expires_in")] public int RefreshTokenExpiresIn { get; set; }
}

internal sealed record QboTokenError
{
    [JsonPropertyName("error")] public string? Error { get; set; }
}

// --- Errors: {"Fault":{"Error":[{"Message","Detail","code"}],"type"}} (casing varies; reads are case-insensitive) ---

internal sealed record QboFaultResponse
{
    public QboFault? Fault { get; set; }
}

internal sealed record QboFault
{
    public List<QboFaultError>? Error { get; set; }
    public string? Type { get; set; }
}

internal sealed record QboFaultError
{
    public string? Message { get; set; }
    public string? Detail { get; set; }
    public string? Code { get; set; }
}

// --- Reference type (QBO uses lowercase "value"/"name"/"type") ---

internal sealed record QboRef
{
    [JsonPropertyName("value")] public string? Value { get; set; }

    /// <summary>Only on polymorphic refs such as Purchase.EntityRef ("Vendor", "Customer", "Employee").</summary>
    [JsonPropertyName("type")] public string? Type { get; set; }

    public static QboRef Of(string value) => new() { Value = value };

    public static QboRef? OfOptional(string? value) => string.IsNullOrEmpty(value) ? null : new QboRef { Value = value };
}

// --- Common pieces ---

/// <summary>Every QBO transaction and name-list entity carries an Id and an optimistic-concurrency SyncToken.</summary>
internal interface IQboEntity
{
    string? Id { get; }
    string? SyncToken { get; }
}

internal sealed record QboEmail
{
    public string? Address { get; set; }
}

internal sealed record QboPhone
{
    public string? FreeFormNumber { get; set; }
}

internal sealed record QboPhysicalAddress
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? CountrySubDivisionCode { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

// --- Customer / Vendor ---

internal sealed record QboCustomer : IQboEntity
{
    public string? Id { get; set; }
    public string? SyncToken { get; set; }

    /// <summary>On updates: only the fields sent are changed, so a linked customer keeps what we do not own.</summary>
    [JsonPropertyName("sparse")] public bool? Sparse { get; set; }

    public bool? Active { get; set; }
    public string? DisplayName { get; set; }
    public QboEmail? PrimaryEmailAddr { get; set; }
    public QboPhone? PrimaryPhone { get; set; }
    public QboPhysicalAddress? BillAddr { get; set; }
}

internal sealed record QboVendor : IQboEntity
{
    public string? Id { get; set; }
    public string? SyncToken { get; set; }
    [JsonPropertyName("sparse")] public bool? Sparse { get; set; }
    public bool? Active { get; set; }
    public string? DisplayName { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public QboEmail? PrimaryEmailAddr { get; set; }
    public QboPhone? PrimaryPhone { get; set; }
}

// --- Lines ---

internal sealed record QboLine
{
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string? DetailType { get; set; }
    public QboSalesItemLineDetail? SalesItemLineDetail { get; set; }
    public QboAccountBasedExpenseLineDetail? AccountBasedExpenseLineDetail { get; set; }
    public List<QboLinkedTxn>? LinkedTxn { get; set; }
}

internal sealed record QboSalesItemLineDetail
{
    public decimal? Qty { get; set; }
    public decimal? UnitPrice { get; set; }
    public QboRef? ItemRef { get; set; }
}

internal sealed record QboAccountBasedExpenseLineDetail
{
    public QboRef? AccountRef { get; set; }
}

internal sealed record QboLinkedTxn
{
    public string? TxnId { get; set; }
    public string? TxnType { get; set; }
}

internal sealed record QboTxnTaxDetail
{
    public decimal? TotalTax { get; set; }
}

// --- Invoice ---

internal sealed record QboInvoice : IQboEntity
{
    public string? Id { get; set; }
    public string? SyncToken { get; set; }
    [JsonPropertyName("sparse")] public bool? Sparse { get; set; }
    public string? DocNumber { get; set; }
    public string? TxnDate { get; set; }
    public string? DueDate { get; set; }
    public QboRef? CustomerRef { get; set; }
    public QboRef? CurrencyRef { get; set; }
    public QboTxnTaxDetail? TxnTaxDetail { get; set; }
    public List<QboLine>? Line { get; set; }
}

// --- Payment ---

internal sealed record QboPayment : IQboEntity
{
    public string? Id { get; set; }
    public string? SyncToken { get; set; }
    [JsonPropertyName("sparse")] public bool? Sparse { get; set; }
    public string? TxnDate { get; set; }
    public QboRef? CustomerRef { get; set; }
    public QboRef? CurrencyRef { get; set; }
    public QboRef? DepositToAccountRef { get; set; }
    public decimal TotalAmt { get; set; }
    public string? PaymentRefNum { get; set; }
    public List<QboLine>? Line { get; set; }
}

// --- Purchase (expense) ---

internal sealed record QboPurchase : IQboEntity
{
    public string? Id { get; set; }
    public string? SyncToken { get; set; }
    [JsonPropertyName("sparse")] public bool? Sparse { get; set; }
    public QboRef? AccountRef { get; set; }
    public QboRef? EntityRef { get; set; }
    public QboRef? CurrencyRef { get; set; }

    /// <summary>"Cash", "Check" or "CreditCard"; must agree with the AccountRef's account type.</summary>
    public string? PaymentType { get; set; }

    public string? TxnDate { get; set; }
    public string? DocNumber { get; set; }
    public string? PrivateNote { get; set; }
    public List<QboLine>? Line { get; set; }
}

// --- Bill (driver pay) ---

internal sealed record QboBill : IQboEntity
{
    public string? Id { get; set; }
    public string? SyncToken { get; set; }
    [JsonPropertyName("sparse")] public bool? Sparse { get; set; }
    public QboRef? VendorRef { get; set; }
    public QboRef? CurrencyRef { get; set; }
    public string? TxnDate { get; set; }
    public string? DueDate { get; set; }
    public string? DocNumber { get; set; }
    public string? PrivateNote { get; set; }
    public List<QboLine>? Line { get; set; }
}

/// <summary>
/// Write/read response envelope. QBO wraps the entity under its own name ({"Invoice": {...}}), so one
/// record covers every resource we write.
/// </summary>
internal sealed record QboEntityResponse
{
    public QboCustomer? Customer { get; set; }
    public QboVendor? Vendor { get; set; }
    public QboInvoice? Invoice { get; set; }
    public QboPayment? Payment { get; set; }
    public QboPurchase? Purchase { get; set; }
    public QboBill? Bill { get; set; }
}

// --- Lists ---

internal sealed record QboAccount
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? AccountType { get; set; }
    public string? Classification { get; set; }
}

internal sealed record QboItem
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
}

internal sealed record QboCompanyInfo
{
    public string? CompanyName { get; set; }
}

internal sealed record QboQueryResponse
{
    public QboQueryEnvelope? QueryResponse { get; set; }
}

internal sealed record QboQueryEnvelope
{
    public List<QboAccount>? Account { get; set; }
    public List<QboItem>? Item { get; set; }
    public List<QboCustomer>? Customer { get; set; }
    public List<QboVendor>? Vendor { get; set; }
    public List<QboCompanyInfo>? CompanyInfo { get; set; }
}
