using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Infrastructure.Integrations.Accounting.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;

/// <summary>
///     Handles the QuickBooks Online OAuth2 flow: building the authorization URL, exchanging the
///     authorization code for tokens, refreshing, and revoking. Also resolves the connected
///     company name for display.
/// </summary>
internal sealed class QuickBooksOAuthClient(
    HttpClient httpClient,
    IOptions<AccountingOptions> options,
    ILogger<QuickBooksOAuthClient> logger)
{
    private QuickBooksOptions Config =>
        options.Value.QuickBooks ?? throw new InvalidOperationException("QuickBooks options are not configured.");

    public string BuildAuthorizationUrl(string state, string redirectUri)
    {
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = Config.ClientId,
            ["response_type"] = "code",
            ["scope"] = Config.Scopes,
            ["redirect_uri"] = redirectUri,
            ["state"] = state
        };

        var qs = string.Join('&', query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value ?? string.Empty)}"));
        return $"{QboEndpoints.AuthorizeUrl}?{qs}";
    }

    public async Task<QboTokenResponse> ExchangeCodeAsync(string code, string redirectUri, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri
        };
        return await PostTokenAsync(form, "code exchange", ct);
    }

    public async Task<QboTokenResponse> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };
        return await PostTokenAsync(form, "token refresh", ct);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, QboEndpoints.RevokeUrl)
        {
            Content = JsonContent.Create(new { token = refreshToken })
        };
        request.Headers.Authorization = BasicAuth();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            // Revoke is best-effort; log and continue so a local disconnect still succeeds.
            logger.LogWarning("QBO token revoke returned {StatusCode} (intuit_tid {IntuitTid})",
                response.StatusCode, response.GetIntuitTid());
        }
    }

    public async Task<string?> GetCompanyNameAsync(string realmId, string accessToken, string environment, CancellationToken ct)
    {
        try
        {
            var url = QboEndpoints.QueryResource(environment, realmId, "select * from CompanyInfo");
            var result = await httpClient.GetQboAsync<QboQueryResponse>(url, accessToken, "get company info", ct);
            return result.QueryResponse?.CompanyInfo?.FirstOrDefault()?.CompanyName;
        }
        catch (QboApiException ex)
        {
            logger.LogWarning(ex, "Could not resolve QBO company name for realm {RealmId}", realmId);
            return null;
        }
    }

    private async Task<QboTokenResponse> PostTokenAsync(
        Dictionary<string, string> form, string action, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, QboEndpoints.TokenUrl)
        {
            Content = new FormUrlEncodedContent(form)
        };
        request.Headers.Authorization = BasicAuth();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            var error = new QboApiException(action, response.StatusCode, body, response.GetIntuitTid());
            logger.LogWarning("QBO {Action} failed with {StatusCode} (intuit_tid {IntuitTid})",
                action, response.StatusCode, error.IntuitTid);

            // Intuit answers an expired or revoked refresh token (and a reused code) with invalid_grant.
            throw IsInvalidGrant(body)
                ? new AccountingReconnectRequiredException("QuickBooks refused the stored refresh token.", error)
                : error;
        }

        var token = await response.Content.ReadFromJsonAsync<QboTokenResponse>(QboJsonOptions.Default, ct);
        return token ?? throw new QboApiException(action, response.StatusCode, "empty token response", response.GetIntuitTid());
    }

    private static bool IsInvalidGrant(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<QboTokenError>(body, QboJsonOptions.Default)?.Error == "invalid_grant";
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private AuthenticationHeaderValue BasicAuth()
    {
        var raw = $"{Config.ClientId}:{Config.ClientSecret}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        return new AuthenticationHeaderValue("Basic", encoded);
    }
}
