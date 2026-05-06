using Logistics.Domain.Core;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Tenant-managed tax rate used by <c>ManualTaxCalculator</c> when Stripe Tax is not enabled.
/// Lives in the master database so rates survive tenant DB rebuilds.
/// </summary>
public class TenantTaxRate : AuditableEntity, IMasterEntity
{
    public required Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    public required TaxJurisdiction Jurisdiction { get; set; }

    /// <summary>
    /// Rate expressed as a percentage (e.g. 19.00 for 19%).
    /// </summary>
    public required decimal RatePercent { get; set; }

    /// <summary>
    /// Stripe Tax product code (txcd_*) — optional. When set, narrows the rate to a specific
    /// product/service category. When null the rate applies to any line item in the jurisdiction.
    /// </summary>
    public string? TaxCode { get; set; }

    public string? Description { get; set; }

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }

    public bool IsActiveOn(DateTime instant) =>
        instant >= EffectiveFrom && (EffectiveTo is null || instant < EffectiveTo);
}
