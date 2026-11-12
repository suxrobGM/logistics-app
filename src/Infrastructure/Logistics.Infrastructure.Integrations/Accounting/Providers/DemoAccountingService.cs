using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Integrations.Accounting.Providers;

/// <summary>
///     No-op accounting provider for demo tenants. Echoes deterministic fake IDs so the sync
///     pipeline can be exercised without a real QuickBooks connection.
/// </summary>
internal sealed class DemoAccountingService : IAccountingProviderService
{
    public AccountingProviderType ProviderType => AccountingProviderType.Demo;

    public void Initialize(AccountingProviderConfiguration configuration)
    {
    }

    public string BuildAuthorizationUrl(string state) => $"about:blank#demo-{state}";

    public Task<AccountingConnectionResultDto> ExchangeCodeAsync(
        string code, string realmId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return Task.FromResult(new AccountingConnectionResultDto
        {
            AccessToken = "demo-access-token",
            RefreshToken = "demo-refresh-token",
            AccessTokenExpiresAt = now.AddHours(1),
            RefreshTokenExpiresAt = now.AddDays(100),
            RealmId = string.IsNullOrEmpty(realmId) ? "demo-realm" : realmId,
            CompanyName = "Demo Company"
        });
    }

    public Task<OAuthTokenResultDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default) =>
        Task.FromResult(new OAuthTokenResultDto
        {
            AccessToken = "demo-access-token",
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(100)
        });

    public Task RevokeAsync(string refreshToken, CancellationToken ct = default) => Task.CompletedTask;

    public Task<IReadOnlyList<QboAccountDto>> GetAccountsAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<QboAccountDto>>(
        [
            new QboAccountDto { Id = "35", Name = "Checking", AccountType = "Bank", Classification = "Asset" },
            new QboAccountDto { Id = "41", Name = "Fuel Card", AccountType = "Credit Card", Classification = "Liability" },
            new QboAccountDto { Id = "60", Name = "Job Expenses", AccountType = "Expense", Classification = "Expense" },
            new QboAccountDto { Id = "61", Name = "Fuel", AccountType = "Expense", Classification = "Expense" },
            new QboAccountDto { Id = "62", Name = "Driver Pay", AccountType = "Cost of Goods Sold", Classification = "Expense" }
        ]);

    public Task<IReadOnlyList<QboItemDto>> GetItemsAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<QboItemDto>>(
        [
            new QboItemDto { Id = "1", Name = "Services", Type = "Service" },
            new QboItemDto { Id = "2", Name = "Freight", Type = "Service" },
            new QboItemDto { Id = "3", Name = "Fuel Surcharge", Type = "Service" }
        ]);

    public Task<QboUpsertResult> UpsertCustomerAsync(QboCustomerPayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> DeactivateCustomerAsync(QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> UpsertVendorAsync(QboVendorPayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> UpsertInvoiceAsync(QboInvoicePayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> VoidInvoiceAsync(QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> UpsertPaymentAsync(QboPaymentPayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> VoidPaymentAsync(QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> UpsertExpenseAsync(QboExpensePayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    public Task<QboUpsertResult> UpsertBillAsync(QboBillPayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        Fake(target);

    private static Task<QboUpsertResult> Fake(QboWriteTarget target) =>
        Task.FromResult(new QboUpsertResult
        {
            QboId = target.QboId ?? Guid.NewGuid().ToString("N")[..8],
            SyncToken = "0"
        });
}
