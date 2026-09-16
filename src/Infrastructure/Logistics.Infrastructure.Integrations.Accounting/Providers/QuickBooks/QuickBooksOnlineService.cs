using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Accounting.Common;
using Logistics.Shared.Models;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;

/// <summary>
///     QuickBooks Online implementation of the accounting port. One-way push via the QBO v3
///     Data API; OAuth handled by <see cref="QuickBooksOAuthClient"/>.
/// </summary>
internal sealed class QuickBooksOnlineService(
    HttpClient httpClient,
    QuickBooksOAuthClient oauthClient,
    IOptions<AccountingOptions> options) : IAccountingProviderService
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

    public async Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var token = await oauthClient.RefreshAsync(refreshToken, ct);
        return new OAuthTokenResultDto
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn)
        };
    }

    public Task RevokeAsync(string refreshToken, CancellationToken ct = default) =>
        oauthClient.RevokeAsync(refreshToken, ct);

    public async Task<IReadOnlyList<QboAccountDto>> GetAccountsAsync(CancellationToken ct = default)
    {
        var url = QboEndpoints.QueryResource(Environment, RequireRealm(), "select * from Account maxresults 1000");
        var result = await httpClient.GetQboAsync<QboQueryResponse<QboAccount>>(url, RequireToken(), "get accounts", ct);
        var accounts = result.QueryResponse?.Account ?? [];
        return accounts
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

    public async Task<QboUpsertResult> UpsertCustomerAsync(
        QboCustomerPayload payload, string? qboId, string? syncToken, CancellationToken ct = default)
    {
        var body = QboMapper.MapCustomer(payload, qboId, syncToken);
        var response = await PostEntityAsync<QboCustomerResponse>("customer", body, "upsert customer", ct);
        return ToResult(response.Customer?.Id, response.Customer?.SyncToken);
    }

    public async Task<QboUpsertResult> UpsertInvoiceAsync(
        QboInvoicePayload payload, string? qboId, string? syncToken, CancellationToken ct = default)
    {
        var body = QboMapper.MapInvoice(payload, qboId, syncToken);
        var response = await PostEntityAsync<QboInvoiceResponse>("invoice", body, "upsert invoice", ct);
        return ToResult(response.Invoice?.Id, response.Invoice?.SyncToken);
    }

    public async Task<QboUpsertResult> UpsertPaymentAsync(
        QboPaymentPayload payload, string? qboId, string? syncToken, CancellationToken ct = default)
    {
        var body = QboMapper.MapPayment(payload, qboId, syncToken);
        var response = await PostEntityAsync<QboPaymentResponse>("payment", body, "upsert payment", ct);
        return ToResult(response.Payment?.Id, response.Payment?.SyncToken);
    }

    public async Task<QboUpsertResult> UpsertExpenseAsync(
        QboExpensePayload payload, string? qboId, string? syncToken, CancellationToken ct = default)
    {
        var body = QboMapper.MapPurchase(payload, qboId, syncToken);
        var response = await PostEntityAsync<QboPurchaseResponse>("purchase", body, "upsert expense", ct);
        return ToResult(response.Purchase?.Id, response.Purchase?.SyncToken);
    }

    private Task<TResponse> PostEntityAsync<TResponse>(
        string resource, object body, string action, CancellationToken ct)
    {
        var url = QboEndpoints.CompanyResource(Environment, RequireRealm(), resource);
        return httpClient.PostQboAsync<TResponse>(url, body, RequireToken(), action, ct);
    }

    private static QboUpsertResult ToResult(string? id, string? syncToken)
    {
        if (id is null)
        {
            throw new InvalidOperationException("QBO response did not contain an entity Id.");
        }

        return new QboUpsertResult { QboId = id, SyncToken = syncToken };
    }

    private string RequireRealm() =>
        _realmId ?? throw new InvalidOperationException("QuickBooks service is not initialized (missing realm ID).");

    private string RequireToken() =>
        _accessToken ?? throw new InvalidOperationException("QuickBooks service is not initialized (missing access token).");
}
