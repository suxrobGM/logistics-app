using Logistics.Infrastructure.Tax.Stripe;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Tax.Tests.Stripe;

public class StripeTaxConfigServiceTests
{
    private readonly IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

    private readonly TaxOptions options = new()
    {
        Provider = "stripe",
        FallbackTaxCode = "txcd_fallback_42",
        StripeConfigCacheMinutes = 15
    };

    private StripeTaxConfigService NewSut()
    {
        return new StripeTaxConfigService(
            Options.Create(options),
            cache,
            NullLogger<StripeTaxConfigService>.Instance);
    }

    [Fact]
    public async Task GetDefaultTaxCodeAsync_CacheHit_ReturnsCached_WithoutCallingStripe()
    {
        // Pre-warm the cache to bypass the Stripe SDK call entirely.
        cache.Set("tax:stripe:default_code", "txcd_cached_value");

        var sut = NewSut();
        var code = await sut.GetDefaultTaxCodeAsync();

        Assert.Equal("txcd_cached_value", code);
    }

    [Fact]
    public async Task GetDefaultTaxCodeAsync_StripeFails_ReturnsFallbackAndShortCachesIt()
    {
        // No cache entry, no API key configured → SettingsService.GetAsync throws.
        var sut = NewSut();

        var code = await sut.GetDefaultTaxCodeAsync();

        Assert.Equal("txcd_fallback_42", code);
        // Subsequent call hits the short-TTL fallback cache.
        Assert.True(cache.TryGetValue("tax:stripe:default_code", out string? cached));
        Assert.Equal("txcd_fallback_42", cached);
    }
}
