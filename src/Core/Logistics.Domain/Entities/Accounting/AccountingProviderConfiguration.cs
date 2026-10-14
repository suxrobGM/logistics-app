using System.Text.Json;
using System.Text.Json.Serialization;
using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Per-tenant configuration for an accounting provider integration (e.g. QuickBooks Online).
/// Holds the OAuth2 connection, the account/item choices pushes post to, and sync bookkeeping.
/// </summary>
public class AccountingProviderConfiguration : Entity, ITenantEntity
{
    private static readonly JsonSerializerOptions MappingsJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public required AccountingProviderType ProviderType { get; set; }

    /// <summary>
    /// The accounting company/realm ID in the provider's system (QBO "realmId").
    /// </summary>
    public string? RealmId { get; set; }

    /// <summary>
    /// Human-readable company name from the provider, for display in settings.
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// OAuth2 access token (encrypted at rest).
    /// </summary>
    [EncryptedSecret]
    public string? AccessToken { get; set; }

    /// <summary>
    /// OAuth2 refresh token used to renew the access token (encrypted at rest).
    /// </summary>
    [EncryptedSecret]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the current access token expires.
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// When the refresh token expires (QBO refresh tokens last ~100 days and roll forward on every refresh).
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// The provider rejected the stored tokens (refresh token expired or revoked, or the user disconnected
    /// the app on the provider's side). Sync stops until the user connects again.
    /// </summary>
    public bool NeedsReconnect { get; set; }

    /// <summary>
    /// Bank or credit card account expenses are paid from (QBO Purchase.AccountRef).
    /// </summary>
    public string? DefaultPaymentAccountId { get; set; }

    /// <summary>
    /// Provider account type of <see cref="DefaultPaymentAccountId"/> (QBO "Bank" or "Credit Card"),
    /// which decides the purchase payment type.
    /// </summary>
    public string? DefaultPaymentAccountType { get; set; }

    /// <summary>
    /// Expense account for expense lines whose category has no mapping.
    /// </summary>
    public string? DefaultExpenseAccountId { get; set; }

    /// <summary>
    /// Sales item for invoice lines whose type has no mapping.
    /// </summary>
    public string? DefaultItemId { get; set; }

    /// <summary>
    /// Account customer payments are deposited to. Null leaves QBO's default (Undeposited Funds).
    /// </summary>
    public string? DepositAccountId { get; set; }

    /// <summary>
    /// Expense account driver pay bills post to. Null skips driver pay sync.
    /// </summary>
    public string? PayrollExpenseAccountId { get; set; }

    /// <summary>
    /// Transactions (invoices, payments, expenses, driver pay) dated before this are never pushed, so
    /// history already entered in the accounting system is not duplicated. Set to the connect date on
    /// the first connection; the user can move it.
    /// </summary>
    public DateTime? SyncFromDate { get; set; }

    /// <summary>
    /// Persisted JSON of <see cref="AccountingMappings"/>. Use <see cref="GetMappings"/> /
    /// <see cref="SetMappings"/> to access.
    /// </summary>
    public string? MappingsJson { get; set; }

    /// <summary>
    /// Whether this provider configuration is active (a disconnect deactivates it).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When data was last pushed to this provider.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    public AccountingMappings GetMappings() =>
        string.IsNullOrWhiteSpace(MappingsJson)
            ? new AccountingMappings()
            : JsonSerializer.Deserialize<AccountingMappings>(MappingsJson, MappingsJsonOptions) ?? new AccountingMappings();

    public void SetMappings(AccountingMappings mappings) =>
        MappingsJson = JsonSerializer.Serialize(mappings, MappingsJsonOptions);

    /// <summary>
    /// Store a completed OAuth connection. Connecting a different company than before clears every
    /// account and item choice, since those ids belong to the old company. Returns true in that case so
    /// the caller can also drop the old company's entity mappings.
    /// </summary>
    public bool Connect(
        string realmId, string? companyName, string accessToken, string refreshToken,
        DateTime accessTokenExpiresAt, DateTime refreshTokenExpiresAt)
    {
        var companyChanged = RealmId is not null && RealmId != realmId;
        if (RealmId is null || companyChanged)
        {
            SyncFromDate = DateTime.UtcNow.Date;
        }

        if (companyChanged)
        {
            DefaultPaymentAccountId = null;
            DefaultPaymentAccountType = null;
            DefaultExpenseAccountId = null;
            DefaultItemId = null;
            DepositAccountId = null;
            PayrollExpenseAccountId = null;
            MappingsJson = null;
            LastSyncedAt = null;
        }

        RealmId = realmId;
        CompanyName = companyName;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        TokenExpiresAt = accessTokenExpiresAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        NeedsReconnect = false;
        IsActive = true;
        return companyChanged;
    }

    public void Disconnect()
    {
        IsActive = false;
        NeedsReconnect = false;
        AccessToken = null;
        RefreshToken = null;
    }
}
