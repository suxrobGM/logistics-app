using Stripe.Tax;

namespace Logistics.Infrastructure.Tax.Stripe;

/// <summary>
/// One-method seam over <see cref="CalculationService"/> so the calculator can be unit-tested
/// without touching the Stripe SDK's static configuration or the network.
/// </summary>
internal interface IStripeTaxCalculationApi
{
    Task<Calculation> CreateAsync(CalculationCreateOptions options, CancellationToken ct);
}
