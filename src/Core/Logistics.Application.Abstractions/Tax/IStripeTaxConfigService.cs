using Logistics.Application.Abstractions.Tax;
namespace Logistics.Application.Abstractions.Tax;

/// <summary>
/// Reads tenant-level tax configuration from Stripe (cached): the default Stripe tax code
/// used when individual line items don't specify one. Implementation lives in
/// <c>Logistics.Infrastructure.Tax</c>.
/// </summary>
public interface IStripeTaxConfigService
{
    /// <summary>Default <c>txcd_*</c> code from <c>Tax.Settings.Defaults.TaxCode</c>; falls back to TaxOptions.</summary>
    Task<string> GetDefaultTaxCodeAsync(CancellationToken ct = default);
}
