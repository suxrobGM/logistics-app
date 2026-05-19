namespace Logistics.Application.Abstractions.Tax;

/// <summary>
/// Computes per-line and aggregate tax for an invoice. Two implementations live in
/// <c>Logistics.Infrastructure.Tax</c>: <c>StripeTaxCalculator</c> (default) and
/// <c>ManualTaxCalculator</c> (fallback when Stripe Tax is not enabled). Selection is
/// driven by <c>TaxOptions.Provider</c>.
/// </summary>
public interface ITaxCalculator
{
    Task<TaxCalculationResult> CalculateAsync(TaxCalculationRequest request, CancellationToken ct = default);
}
