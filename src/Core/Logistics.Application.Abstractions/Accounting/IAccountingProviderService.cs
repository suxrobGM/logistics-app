using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.Accounting;

/// <summary>
/// Port for an accounting provider integration (e.g. QuickBooks Online). One-way push:
/// the TMS is the source of truth and creates/updates records in the provider.
/// </summary>
/// <remarks>
/// Every call throws <see cref="AccountingReconnectRequiredException"/> when the provider rejects the
/// stored tokens, and another exception (with the provider's error) when a single write fails.
/// </remarks>
public interface IAccountingProviderService
{
    /// <summary>The provider this service handles.</summary>
    AccountingProviderType ProviderType { get; }

    /// <summary>Initialize the service with an existing tenant connection (tokens, realm).</summary>
    void Initialize(AccountingProviderConfiguration configuration);

    /// <summary>
    /// Build the provider's OAuth2 authorization URL to which the user is redirected to grant
    /// access. The redirect URI is taken from the provider's own configuration.
    /// </summary>
    string BuildAuthorizationUrl(string state);

    /// <summary>
    /// Exchange an OAuth2 authorization code (from the redirect callback) for tokens and the
    /// connected company details.
    /// </summary>
    Task<AccountingConnectionResultDto> ExchangeCodeAsync(
        string code, string realmId, CancellationToken ct = default);

    /// <summary>Refresh an access token using a refresh token; returns the rotated tokens.</summary>
    Task<OAuthTokenResultDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Revoke the connection at the provider (called on disconnect).</summary>
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>The connected company's active Chart of Accounts.</summary>
    Task<IReadOnlyList<QboAccountDto>> GetAccountsAsync(CancellationToken ct = default);

    /// <summary>The connected company's active products and services.</summary>
    Task<IReadOnlyList<QboItemDto>> GetItemsAsync(CancellationToken ct = default);

    /// <summary>
    /// Create or update a customer. A create first looks for a provider customer with the same name and
    /// links to it, since provider display names are unique.
    /// </summary>
    Task<QboUpsertResult> UpsertCustomerAsync(QboCustomerPayload payload, QboWriteTarget target, CancellationToken ct = default);

    /// <summary>Mark a pushed customer inactive (the TMS customer was deactivated or deleted).</summary>
    Task<QboUpsertResult> DeactivateCustomerAsync(QboWriteTarget target, CancellationToken ct = default);

    /// <summary>Create or update a vendor, linking to an existing one with the same name on create.</summary>
    Task<QboUpsertResult> UpsertVendorAsync(QboVendorPayload payload, QboWriteTarget target, CancellationToken ct = default);

    /// <summary>Create or update an invoice.</summary>
    Task<QboUpsertResult> UpsertInvoiceAsync(QboInvoicePayload payload, QboWriteTarget target, CancellationToken ct = default);

    /// <summary>Void a pushed invoice (the TMS invoice was cancelled).</summary>
    Task<QboUpsertResult> VoidInvoiceAsync(QboWriteTarget target, CancellationToken ct = default);

    /// <summary>Create or update a payment linked to a pushed invoice.</summary>
    Task<QboUpsertResult> UpsertPaymentAsync(QboPaymentPayload payload, QboWriteTarget target, CancellationToken ct = default);

    /// <summary>Create or update an expense (QBO Purchase).</summary>
    Task<QboUpsertResult> UpsertExpenseAsync(QboExpensePayload payload, QboWriteTarget target, CancellationToken ct = default);

    /// <summary>Create or update a bill owed to a vendor (driver pay).</summary>
    Task<QboUpsertResult> UpsertBillAsync(QboBillPayload payload, QboWriteTarget target, CancellationToken ct = default);
}
