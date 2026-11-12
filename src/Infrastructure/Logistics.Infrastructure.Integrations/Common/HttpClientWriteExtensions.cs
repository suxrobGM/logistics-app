using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.Common;

/// <summary>
/// Write-side HttpClient extension shared by the integrations. The read side lives in
/// <see cref="HttpClientJsonExtensions"/>. Like it, this never throws - a failure is logged and
/// surfaced as <c>false</c>. Push paths that must surface an error to the caller (e.g. the
/// QuickBooks helpers in Integrations.Accounting) deliberately do NOT belong here.
/// </summary>
public static class HttpClientWriteExtensions
{
    extension(HttpClient client)
    {
        public async Task<bool> TryPostAsync<TReq>(
            string url,
            TReq body,
            ILogger logger,
            string action,
            CancellationToken ct = default)
        {
            try
            {
                using var response = await client.PostAsJsonAsync(url, body, ct);
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
    }

}
