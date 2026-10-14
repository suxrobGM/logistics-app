using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Logistics.Application.Abstractions.Accounting;

namespace Logistics.Infrastructure.Integrations.Accounting.Common;

/// <summary>
///     HttpClient extensions for the QuickBooks Online Data API. Unlike the ELD read helpers,
///     these throw <see cref="QboApiException"/> on failure so a push error surfaces to the
///     sync job (which records it against the entity mapping) rather than being swallowed.
///     A 401 means Intuit no longer accepts the token (the user disconnected the app in QuickBooks)
///     and surfaces as <see cref="AccountingReconnectRequiredException"/>.
/// </summary>
internal static class HttpClientQboExtensions
{
    public const string IntuitTidHeader = "intuit_tid";

    public static async Task<TResponse> GetQboAsync<TResponse>(
        this HttpClient client, string url, string accessToken, string action, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        Authorize(request, accessToken);
        return await SendAsync<TResponse>(client, request, action, ct);
    }

    public static async Task<TResponse> PostQboAsync<TResponse>(
        this HttpClient client, string url, object payload, string accessToken, string action, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload, payload.GetType(), options: QboJsonOptions.Default)
        };
        Authorize(request, accessToken);
        return await SendAsync<TResponse>(client, request, action, ct);
    }

    public static string? GetIntuitTid(this HttpResponseMessage response) =>
        response.Headers.TryGetValues(IntuitTidHeader, out var values) ? values.FirstOrDefault() : null;

    private static void Authorize(HttpRequestMessage request, string accessToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static async Task<TResponse> SendAsync<TResponse>(
        HttpClient client, HttpRequestMessage request, string action, CancellationToken ct)
    {
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            var error = new QboApiException(action, response.StatusCode, body, response.GetIntuitTid());
            throw response.StatusCode == HttpStatusCode.Unauthorized
                ? new AccountingReconnectRequiredException("QuickBooks no longer accepts this connection.", error)
                : error;
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(QboJsonOptions.Default, ct);
        return result ?? throw new QboApiException(action, response.StatusCode, "empty response body", response.GetIntuitTid());
    }
}
