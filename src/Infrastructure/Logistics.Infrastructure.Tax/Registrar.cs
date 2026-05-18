using Logistics.Infrastructure.Tax.Data;
using Logistics.Infrastructure.Tax.Manual;
using Logistics.Infrastructure.Tax.Stripe;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Infrastructure.Tax;

public static class Registrar
{
    /// <summary>
    /// Registers <c>ITaxCalculator</c> per <c>Tax:Provider</c> ("stripe" | "manual").
    /// Defaults to Stripe. Always registers <c>IStripeTaxConfigService</c> so the API can
    /// expose Stripe Tax onboarding info regardless of which calculator is selected.
    /// </summary>
    public static IServiceCollection AddTaxInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TaxOptions>(configuration.GetSection(TaxOptions.SectionName));
        services.AddMemoryCache();

        services.AddSingleton<IStripeTaxCalculationApi, StripeTaxCalculationApi>();
        services.AddScoped<IStripeTaxConfigService, StripeTaxConfigService>();
        services.AddSingleton<ITaxJurisdictionsProvider, TaxJurisdictionsProvider>();
        services.AddScoped<ManualTaxCalculator>();
        services.AddScoped<StripeTaxCalculator>();

        services.AddScoped<ITaxCalculator>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TaxOptions>>().Value;
            return options.Provider?.Equals("manual", StringComparison.OrdinalIgnoreCase) == true
                ? sp.GetRequiredService<ManualTaxCalculator>()
                : sp.GetRequiredService<StripeTaxCalculator>();
        });

        return services;
    }
}
