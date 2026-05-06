using Stripe.Tax;

namespace Logistics.Infrastructure.Tax.Stripe;

internal sealed class StripeTaxCalculationApi : IStripeTaxCalculationApi
{
    private readonly CalculationService service = new();

    public Task<Calculation> CreateAsync(CalculationCreateOptions options, CancellationToken ct) =>
        service.CreateAsync(options, cancellationToken: ct);
}
