using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.FuelCards.Common;

/// <summary>
/// HttpClient extensions used by fuel card provider services. Wraps the
/// "send + status check + JSON deserialise + log on failure" pattern.
/// </summary>
internal static class HttpClientFuelCardExtensions
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
                logger.LogWarning("FuelCard {Action} returned {StatusCode}", action, response.StatusCode);
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException or NotSupportedException)
        {
            logger.LogError(ex, "FuelCard {Action} failed", action);
            return default;
        }
    }
}
