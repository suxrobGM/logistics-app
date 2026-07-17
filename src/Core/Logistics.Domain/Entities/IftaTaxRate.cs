using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Published IFTA fuel tax rate for one jurisdiction and quarter. Master-DB because rates
/// are jurisdiction-wide public facts serving all tenants; seeded from JSON each quarter and
/// admin-maintained. Report rows with no matching rate are flagged, never guessed.
/// </summary>
public class IftaTaxRate : AuditableEntity, IMasterEntity
{
    public required TaxJurisdiction Jurisdiction { get; set; }

    public required int Year { get; set; }

    /// <summary>Calendar quarter, 1-4.</summary>
    public required int Quarter { get; set; }

    /// <summary>Tax rate in USD per gallon.</summary>
    public required decimal RatePerGallon { get; set; }

    /// <summary>Additional surcharge per gallon (Indiana, Kentucky, Virginia).</summary>
    public decimal? SurchargeRatePerGallon { get; set; }
}
