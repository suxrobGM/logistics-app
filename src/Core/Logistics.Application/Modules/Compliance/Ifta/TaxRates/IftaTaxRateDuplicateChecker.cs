using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates;

internal sealed class IftaTaxRateDuplicateChecker(IMasterUnitOfWork masterUow) : IIftaTaxRateDuplicateChecker
{
    public async Task<string?> FindConflictAsync(
        TaxJurisdiction jurisdiction, int year, int quarter, Guid? excludeId, CancellationToken ct)
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
