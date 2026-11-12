using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>Never throw: a failure is logged and returns <c>default</c>, so calls that must surface errors don't belong here.</summary>
public static class HttpClientJsonExtensions
{
    extension(HttpClient client)
    {
        /// <summary>Returns <c>default</c> on any failure, logged under the <paramref name="action"/> label.</summary>
        public async Task<T?> TryGetFromJsonAsync<T>(
            string url,
            ILogger logger,
            string action,
            JsonSerializerOptions? options = null,
            CancellationToken ct = default)
        {
            try
            {
                using var response = await client.GetAsync(url, ct);
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
}
