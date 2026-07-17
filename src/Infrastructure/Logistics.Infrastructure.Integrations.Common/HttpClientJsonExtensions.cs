using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// HttpClient JSON helpers shared by the third-party integration providers (load board, fuel
/// card, ELD). These wrap the "send + status check + JSON deserialise + log on failure" pattern
/// so provider services stay focused on URL composition and DTO mapping.
///
/// Note the contract: these never throw — a failure is logged and returns <c>default</c>. Push
/// paths that must surface an error to the caller (e.g. the QuickBooks helpers in
/// Integrations.Accounting) deliberately do NOT belong here.
/// </summary>
public static class HttpClientJsonExtensions
{
    /// <summary>
    /// Performs a GET, deserialises the JSON body to <typeparamref name="T"/> on success, or
    /// returns <c>default</c> on a non-success status, network error, or parse failure. All
    /// failures are logged with the supplied <paramref name="action"/> label; the calling
    /// service's logger category identifies which integration it was.
    /// </summary>
    public static async Task<T?> TryGetFromJsonAsync<T>(
        this HttpClient client,
        string url,
        ILogger logger,
        string action,
        JsonSerializerOptions? options = null,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("{Action} returned {StatusCode}", action, response.StatusCode);
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(options, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "{Action} failed", action);
            return default;
        }
    }
}
