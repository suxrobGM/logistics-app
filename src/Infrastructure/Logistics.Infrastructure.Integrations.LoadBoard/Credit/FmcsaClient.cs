using Logistics.Infrastructure.Integrations.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Credit;

/// <summary>
/// FMCSA QCMobile API client used as the free fallback source for broker authority status.
/// Docs: https://mobile.fmcsa.dot.gov/QCDevsite/
/// </summary>
public class FmcsaClient(
    HttpClient httpClient,
    IOptions<FmcsaOptions> options,
    ILogger<FmcsaClient> logger)
{
    private readonly FmcsaOptions options = options.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.WebKey);

    /// <summary>
    /// Looks up a broker/carrier by MC (docket) number. Returns null when no key is
    /// configured, the docket is unknown, or the API call fails.
    /// </summary>
    public virtual async Task<bool?> GetAuthorityActiveAsync(string mcNumber, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            logger.LogDebug("FMCSA webKey not configured; skipping authority lookup");
            return null;
        }

        var url = $"{options.BaseUrl.TrimEnd('/')}/carriers/docket-number/{Uri.EscapeDataString(mcNumber)}?webKey={Uri.EscapeDataString(options.WebKey!)}";
        var response = await httpClient.TryGetFromJsonAsync<FmcsaDocketResponse>(
            url, logger, $"FMCSA docket lookup {mcNumber}", ct: ct);

        var carrier = response?.Content?.FirstOrDefault()?.Carrier;
        if (carrier is null)
        {
            return null;
        }

        return string.Equals(carrier.AllowedToOperate, "Y", StringComparison.OrdinalIgnoreCase);
    }
}
