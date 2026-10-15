using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Accounting.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;

/// <summary>
///     QuickBooks Online implementation of the accounting port. One-way push via the QBO v3
///     Data API; OAuth handled by <see cref="QuickBooksOAuthClient"/>.
/// </summary>
internal sealed class QuickBooksOnlineService(
    HttpClient httpClient,
    QuickBooksOAuthClient oauthClient,
    IOptions<AccountingOptions> options,
    ILogger<QuickBooksOnlineService> logger) : IAccountingProviderService
{
    private string? _realmId;
    private string? _accessToken;

    private string Environment => options.Value.QuickBooks?.Environment ?? "sandbox";

    public AccountingProviderType ProviderType => AccountingProviderType.QuickBooksOnline;

    public void Initialize(AccountingProviderConfiguration configuration)
    {
        _realmId = configuration.RealmId;
        _accessToken = configuration.AccessToken;
    }

    private string RedirectUri =>
        options.Value.QuickBooks?.RedirectUri
        ?? throw new InvalidOperationException("QuickBooks RedirectUri is not configured.");

    public string BuildAuthorizationUrl(string state) =>
        oauthClient.BuildAuthorizationUrl(state, RedirectUri);

    public async Task<AccountingConnectionResultDto> ExchangeCodeAsync(
        string code, string realm, CancellationToken ct = default)
    {
        var token = await oauthClient.ExchangeCodeAsync(code, RedirectUri, ct);
        var companyName = await oauthClient.GetCompanyNameAsync(realm, token.AccessToken, Environment, ct);
        var now = DateTime.UtcNow;

        return new AccountingConnectionResultDto
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            AccessTokenExpiresAt = now.AddSeconds(token.ExpiresIn),
            RefreshTokenExpiresAt = now.AddSeconds(token.RefreshTokenExpiresIn),
            RealmId = realm,
            CompanyName = companyName
        };
    }

    public async Task<OAuthTokenResultDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await oauthClient.RefreshAsync(refreshToken, ct);
        var now = DateTime.UtcNow;
        return new OAuthTokenResultDto
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresAt = now.AddSeconds(token.ExpiresIn),
            RefreshTokenExpiresAt = token.RefreshTokenExpiresIn > 0 ? now.AddSeconds(token.RefreshTokenExpiresIn) : null
        };
    }

    public Task RevokeAsync(string refreshToken, CancellationToken ct = default) =>
        oauthClient.RevokeAsync(refreshToken, ct);

    public async Task<IReadOnlyList<QboAccountDto>> GetAccountsAsync(CancellationToken ct = default)
    {
        var result = await QueryAsync("select * from Account where Active = true maxresults 1000", "get accounts", ct);
        return (result.Account ?? [])
            .Where(a => a.Id is not null && a.Name is not null)
            .Select(a => new QboAccountDto
            {
                Id = a.Id!,
                Name = a.Name!,
                AccountType = a.AccountType,
                Classification = a.Classification
            })
            .ToList();
    }

    public async Task<IReadOnlyList<QboItemDto>> GetItemsAsync(CancellationToken ct = default)
    {
        var result = await QueryAsync("select * from Item where Active = true maxresults 1000", "get items", ct);
        return (result.Item ?? [])
            .Where(i => i.Id is not null && i.Name is not null && i.Type != "Category")
            .Select(i => new QboItemDto { Id = i.Id!, Name = i.Name!, Type = i.Type })
            .ToList();
    }

    public async Task<QboUpsertResult> UpsertCustomerAsync(
        QboCustomerPayload payload, QboWriteTarget target, CancellationToken ct = default)
    {
        if (target.QboId is null)
        {
            var existing = (await FindByDisplayNameAsync("Customer", payload.Name, ct)).Customer?.FirstOrDefault();
            if (existing?.Id is not null)
            {
                target = target with { QboId = existing.Id, SyncToken = existing.SyncToken };
            }
        }

        return await WriteAsync("customer", target, (id, token) => QboMapper.MapCustomer(payload, id, token),
            r => r.Customer, "upsert customer", ct);
    }

    public Task<QboUpsertResult> DeactivateCustomerAsync(QboWriteTarget target, CancellationToken ct = default) =>
        WriteAsync("customer", target, (id, token) => QboMapper.MapCustomerDeactivation(id!, token),
            r => r.Customer, "deactivate customer", ct);

    public async Task<QboUpsertResult> UpsertVendorAsync(
        QboVendorPayload payload, QboWriteTarget target, CancellationToken ct = default)
    {
        if (target.QboId is null)
        {
            var existing = (await FindByDisplayNameAsync("Vendor", payload.DisplayName, ct)).Vendor?.FirstOrDefault();
            if (existing?.Id is not null)
            {
                target = target with { QboId = existing.Id, SyncToken = existing.SyncToken };
            }
        }

        return await WriteAsync("vendor", target, (id, token) => QboMapper.MapVendor(payload, id, token),
            r => r.Vendor, "upsert vendor", ct);
    }

    public Task<QboUpsertResult> UpsertInvoiceAsync(
        QboInvoicePayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        WriteAsync("invoice", target, (id, token) => QboMapper.MapInvoice(payload, id, token),
            r => r.Invoice, "upsert invoice", ct);

    public Task<QboUpsertResult> VoidInvoiceAsync(QboWriteTarget target, CancellationToken ct = default) =>
        WriteAsync("invoice", target, (id, token) => QboMapper.MapInvoiceVoid(id!, token),
            r => r.Invoice, "void invoice", ct, operation: "void");

    public Task<QboUpsertResult> UpsertPaymentAsync(
        QboPaymentPayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        WriteAsync("payment", target, (id, token) => QboMapper.MapPayment(payload, id, token),
            r => r.Payment, "upsert payment", ct);

    /// <remarks>QBO voids a payment through a sparse update with <c>include=void</c>, not <c>operation=void</c>.</remarks>
    public Task<QboUpsertResult> VoidPaymentAsync(QboWriteTarget target, CancellationToken ct = default) =>
        WriteAsync("payment", target, (id, token) => QboMapper.MapPaymentVoid(id!, token),
            r => r.Payment, "void payment", ct, operation: "update&include=void");

    public Task<QboUpsertResult> UpsertExpenseAsync(
        QboExpensePayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        WriteAsync("purchase", target, (id, token) => QboMapper.MapPurchase(payload, id, token),
            r => r.Purchase, "upsert expense", ct);

    public Task<QboUpsertResult> UpsertBillAsync(
        QboBillPayload payload, QboWriteTarget target, CancellationToken ct = default) =>
        WriteAsync("bill", target, (id, token) => QboMapper.MapBill(payload, id, token),
            r => r.Bill, "upsert bill", ct);

    /// <summary>
    ///     Post a create or update. Creates carry the request id so a retry after a lost response returns
    ///     the first result. An update rejected as stale (the record was edited in QuickBooks since our
    ///     last push) is retried once with the record's current SyncToken - the TMS stays the source of truth.
    /// </summary>
    private async Task<QboUpsertResult> WriteAsync(
        string resource,
        QboWriteTarget target,
        Func<string?, string?, object> build,
        Func<QboEntityResponse, IQboEntity?> pick,
        string action,
        CancellationToken ct,
        string? operation = null)
    {
        var requestId = target.QboId is null ? target.RequestId : null;
        var url = QboEndpoints.CompanyResource(Environment, RequireRealm(), resource, requestId, operation);

        try
        {
            return ToResult(pick(await PostAsync(url, build(target.QboId, target.SyncToken), action, ct)));
        }
        catch (QboApiException ex) when (ex.IsStaleObject && target.QboId is not null)
        {
            logger.LogInformation("QBO {Resource} {QboId} was edited in QuickBooks; retrying with its current SyncToken",
                resource, target.QboId);
            var readUrl = QboEndpoints.CompanyResource(Environment, RequireRealm(), $"{resource}/{target.QboId}");
            var current = pick(await httpClient.GetQboAsync<QboEntityResponse>(readUrl, RequireToken(), $"read {resource}", ct));
            return ToResult(pick(await PostAsync(url, build(target.QboId, current?.SyncToken), action, ct)));
        }
    }

    private Task<QboEntityResponse> PostAsync(string url, object body, string action, CancellationToken ct) =>
        httpClient.PostQboAsync<QboEntityResponse>(url, body, RequireToken(), action, ct);

    private async Task<QboQueryEnvelope> FindByDisplayNameAsync(string entity, string displayName, CancellationToken ct)
    {
        // QBO query strings escape a single quote with a backslash. Active IN (true, false) also finds an
        // inactive record, which a create would otherwise collide with on the unique display name.
        var name = displayName.Replace("\\", "\\\\").Replace("'", "\\'");
        return await QueryAsync(
            $"select * from {entity} where DisplayName = '{name}' and Active in (true, false)",
            $"find {entity.ToLowerInvariant()}", ct);
    }

    private async Task<QboQueryEnvelope> QueryAsync(string query, string action, CancellationToken ct)
    {
        var url = QboEndpoints.QueryResource(Environment, RequireRealm(), query);
        var result = await httpClient.GetQboAsync<QboQueryResponse>(url, RequireToken(), action, ct);
        return result.QueryResponse ?? new QboQueryEnvelope();
    }

    private static QboUpsertResult ToResult(IQboEntity? entity)
    {
        if (entity?.Id is null)
        {
            throw new InvalidOperationException("QBO response did not contain an entity Id.");
        }

        return new QboUpsertResult { QboId = entity.Id, SyncToken = entity.SyncToken };
    }

    private string RequireRealm() =>
        _realmId ?? throw new InvalidOperationException("QuickBooks service is not initialized (missing realm ID).");

    private string RequireToken() =>
        _accessToken ?? throw new InvalidOperationException("QuickBooks service is not initialized (missing access token).");
}
