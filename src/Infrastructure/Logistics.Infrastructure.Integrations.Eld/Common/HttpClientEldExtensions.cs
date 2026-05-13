using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Eld.Common;

/// <summary>
/// HttpClient extensions used by ELD provider services. Centralises the
/// "GET + status check + JSON deserialise + log on failure" pattern so each
/// provider service stays focused on URL composition and DTO mapping.
/// </summary>
internal static class HttpClientEldExtensions
{
    /// <summary>
    /// Performs a GET, deserialises the JSON body to <typeparamref name="T"/> on success,
    /// or returns <c>default</c> on a non-success status, network error, or parse failure.
    /// All failures are logged with the supplied <paramref name="action"/> label. Mirrors
    /// the built-in <c>GetFromJsonAsync</c> but never throws.
    /// </summary>
    public static async Task<T?> TryGetFromJsonAsync<T>(
        this HttpClient client,
        string url,
        JsonSerializerOptions options,
        ILogger logger,
        string action,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ELD {Action} returned {StatusCode}", action, response.StatusCode);
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(options, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "ELD {Action} failed", action);
            return default;
        }
    }
}
