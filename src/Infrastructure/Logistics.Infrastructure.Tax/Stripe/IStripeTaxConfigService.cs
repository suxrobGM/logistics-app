namespace Logistics.Infrastructure.Tax.Stripe;

/// <summary>
/// Reads tenant-level tax configuration from Stripe (cached): default tax code,
/// available tax codes, and the jurisdictions where the tenant is registered to collect tax.
/// </summary>
public interface IStripeTaxConfigService
{
    /// <summary>Default <c>txcd_*</c> code from <c>Tax.Settings.Defaults.TaxCode</c>; falls back to TaxOptions.</summary>
    Task<string> GetDefaultTaxCodeAsync(CancellationToken ct = default);

    /// <summary>All Stripe Tax product codes (Id, Name, Description). Cached 24h.</summary>
    Task<IReadOnlyList<StripeTaxCodeInfo>> ListTaxCodesAsync(CancellationToken ct = default);

    /// <summary>Active tax registrations — jurisdictions where the tenant collects tax.</summary>
    Task<IReadOnlyList<StripeTaxRegistrationInfo>> ListRegistrationsAsync(CancellationToken ct = default);
}

public sealed record StripeTaxCodeInfo(string Id, string Name, string? Description);

public sealed record StripeTaxRegistrationInfo(
    string Id,
    string Country,
    string? State,
    string Status,
    DateTime? ActiveFrom,
    DateTime? ExpiresAt);
