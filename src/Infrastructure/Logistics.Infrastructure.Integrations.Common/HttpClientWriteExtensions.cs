using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// Write-side HttpClient extensions shared by the third-party integration providers. Each method
/// wraps the "send + status check + JSON deserialise + log on failure" pattern so providers stop
/// repeating try/catch + IsSuccessStatusCode boilerplate in every operation.
///
/// The read side lives in <see cref="HttpClientJsonExtensions"/>. Like it, these helpers never
/// throw — a failure is logged and surfaced as a failed <see cref="HttpJsonResult{T}"/> or
/// <c>false</c>. Push paths that must surface an error to the caller (e.g. the QuickBooks helpers
/// in Integrations.Accounting) deliberately do NOT belong here.
/// </summary>
public static class HttpClientWriteExtensions
{
    public static async Task<HttpJsonResult<TResp>> TryPostAsJsonAsync<TReq, TResp>(
        this HttpClient client,
        string url,
        TReq body,
        ILogger logger,
        string action,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, body, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                logger.LogWarning("{Action} failed: {StatusCode} - {Error}", action, response.StatusCode, error);
                return new HttpJsonResult<TResp>(false, default, error, response.StatusCode);
            }

            var value = await response.Content.ReadFromJsonAsync<TResp>(ct);
            return new HttpJsonResult<TResp>(true, value, string.Empty, response.StatusCode);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "{Action} failed", action);
            return new HttpJsonResult<TResp>(false, default, ex.Message, null);
        }
    }

    public static async Task<bool> TryPostAsync<TReq>(
        this HttpClient client,
        string url,
        TReq body,
        ILogger logger,
        string action,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, body, ct);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            logger.LogWarning("{Action} returned {StatusCode}", action, response.StatusCode);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "{Action} failed", action);
            return false;
        }
    }

    public static async Task<bool> TryPutAsync<TReq>(
        this HttpClient client,
        string url,
        TReq body,
        ILogger logger,
        string action,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.PutAsJsonAsync(url, body, ct);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            logger.LogWarning("{Action} returned {StatusCode}", action, response.StatusCode);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "{Action} failed", action);
            return false;
        }
    }

    public static async Task<bool> TryDeleteAsync(
        this HttpClient client,
        string url,
        ILogger logger,
        string action,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.DeleteAsync(url, ct);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            logger.LogWarning("{Action} returned {StatusCode}", action, response.StatusCode);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "{Action} failed", action);
            return false;
        }
    }
}
