using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Tax;

namespace Logistics.Infrastructure.Tax.Stripe;

internal sealed class StripeTaxConfigService(
    IOptions<TaxOptions> taxOptions,
    IMemoryCache cache,
    ILogger<StripeTaxConfigService> logger) : IStripeTaxConfigService
{
    private const string DefaultTaxCodeKey = "tax:stripe:default_code";
    private const string TaxCodesKey = "tax:stripe:codes";
    private const string RegistrationsKey = "tax:stripe:registrations";

    private readonly TimeSpan configTtl = TimeSpan.FromMinutes(taxOptions.Value.StripeConfigCacheMinutes);
    private readonly TimeSpan codesTtl = TimeSpan.FromHours(24);
    private readonly TimeSpan registrationsTtl = TimeSpan.FromMinutes(taxOptions.Value.StripeConfigCacheMinutes);

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

    public async Task<IReadOnlyList<StripeTaxCodeInfo>> ListTaxCodesAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(TaxCodesKey, out IReadOnlyList<StripeTaxCodeInfo>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var service = new TaxCodeService();
            var listOptions = new TaxCodeListOptions { Limit = 100 };
            var result = new List<StripeTaxCodeInfo>();

            await foreach (var code in service.ListAutoPagingAsync(listOptions, cancellationToken: ct))
            {
                result.Add(new StripeTaxCodeInfo(code.Id, code.Name, code.Description));
            }

            cache.Set(TaxCodesKey, (IReadOnlyList<StripeTaxCodeInfo>)result, codesTtl);
            return result;
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Failed to load Stripe tax codes");
            return [];
        }
    }

    public async Task<IReadOnlyList<StripeTaxRegistrationInfo>> ListRegistrationsAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(RegistrationsKey, out IReadOnlyList<StripeTaxRegistrationInfo>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var service = new RegistrationService();
            var listOptions = new RegistrationListOptions { Status = "active", Limit = 100 };
            var result = new List<StripeTaxRegistrationInfo>();

            await foreach (var reg in service.ListAutoPagingAsync(listOptions, cancellationToken: ct))
            {
                result.Add(new StripeTaxRegistrationInfo(
                    reg.Id,
                    reg.Country,
                    State: null, // state lives under reg.CountryOptions.<region>.State; flatten if needed later
                    reg.Status,
                    reg.ActiveFrom,
                    reg.ExpiresAt));
            }

            cache.Set(RegistrationsKey, (IReadOnlyList<StripeTaxRegistrationInfo>)result, registrationsTtl);
            return result;
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Failed to load Stripe tax registrations");
            return [];
        }
    }
}
