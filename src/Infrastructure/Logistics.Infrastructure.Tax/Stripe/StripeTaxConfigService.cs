using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Tax;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Infrastructure.Tax.Stripe;

internal sealed class StripeTaxConfigService(
    IOptions<TaxOptions> taxOptions,
    IMemoryCache cache,
    ILogger<StripeTaxConfigService> logger) : IStripeTaxConfigService
{
    private const string DefaultTaxCodeKey = "tax:stripe:default_code";

    private readonly TimeSpan configTtl = TimeSpan.FromMinutes(taxOptions.Value.StripeConfigCacheMinutes);

    public async Task<string> GetDefaultTaxCodeAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(DefaultTaxCodeKey, out string? cached) && !string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        var fallback = taxOptions.Value.FallbackTaxCode;

        try
        {
            var settings = await new SettingsService().GetAsync(cancellationToken: ct);
            var code = settings?.Defaults?.TaxCode ?? fallback;
            cache.Set(DefaultTaxCodeKey, code, configTtl);
            return code;
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Failed to load Stripe Tax settings; using fallback tax code {Code}", fallback);
            cache.Set(DefaultTaxCodeKey, fallback, TimeSpan.FromMinutes(1));
            return fallback;
        }
    }
}
