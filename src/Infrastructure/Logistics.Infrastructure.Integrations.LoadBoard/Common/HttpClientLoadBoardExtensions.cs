using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Common;

internal sealed record HttpJsonResult<T>(bool IsSuccess, T? Value, string ErrorBody, HttpStatusCode? StatusCode);

/// <summary>
/// HttpClient extensions used by load board provider services. Each method wraps the
/// "send + status check + JSON deserialise + log on failure" pattern so providers stop
/// repeating try/catch + IsSuccessStatusCode boilerplate in every operation.
/// </summary>
internal static class HttpClientLoadBoardExtensions
{
    public static async Task<T?> TryGetFromJsonAsync<T>(
        this HttpClient client,
        string url,
        ILogger logger,
        string action,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("LoadBoard {Action} returned {StatusCode}", action, response.StatusCode);
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "LoadBoard {Action} failed", action);
            return default;
        }
    }

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
                logger.LogWarning("LoadBoard {Action} failed: {StatusCode} - {Error}", action, response.StatusCode, error);
                return new HttpJsonResult<TResp>(false, default, error, response.StatusCode);
            }

            var value = await response.Content.ReadFromJsonAsync<TResp>(ct);
            return new HttpJsonResult<TResp>(true, value, string.Empty, response.StatusCode);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "LoadBoard {Action} failed", action);
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

            logger.LogWarning("LoadBoard {Action} returned {StatusCode}", action, response.StatusCode);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "LoadBoard {Action} failed", action);
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

            logger.LogWarning("LoadBoard {Action} returned {StatusCode}", action, response.StatusCode);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "LoadBoard {Action} failed", action);
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

            logger.LogWarning("LoadBoard {Action} returned {StatusCode}", action, response.StatusCode);
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(ex, "LoadBoard {Action} failed", action);
            return false;
        }
    }
}
