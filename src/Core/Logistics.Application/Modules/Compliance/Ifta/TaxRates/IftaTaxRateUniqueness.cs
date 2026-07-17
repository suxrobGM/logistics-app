using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates;

/// <summary>
/// (Year, Quarter, Jurisdiction) uniqueness for IFTA tax rates. It lives here rather than in a DB
/// unique index because complex-type members cannot participate in one (see
/// IftaTaxRateEntityConfiguration), which makes this the only thing protecting the invariant —
/// every write path must go through it. A duplicate would silently change computed tax due:
/// IftaReportService resolves rates with GroupBy(...).First().
/// </summary>
internal static class IftaTaxRateUniqueness
{
    /// <summary>
    /// Returns an error message when another rate already covers the same quarter and
    /// jurisdiction, or null when the write is safe. Pass <paramref name="excludeId"/> when
    /// updating an existing rate so it does not conflict with itself.
    /// </summary>
    public static async Task<string?> FindConflictAsync(
        IMasterUnitOfWork masterUow,
        TaxJurisdiction jurisdiction,
        int year,
        int quarter,
        Guid? excludeId,
        CancellationToken ct)
    {
        var duplicate = await masterUow.Repository<IftaTaxRate>().GetAsync(
            x => (excludeId == null || x.Id != excludeId) &&
                 x.Year == year && x.Quarter == quarter &&
                 x.Jurisdiction.CountryCode == jurisdiction.CountryCode &&
                 x.Jurisdiction.Region == jurisdiction.Region, ct);

        return duplicate is null
            ? null
            : $"An IFTA tax rate for {duplicate.Jurisdiction} {year} Q{quarter} already exists";
    }
}
