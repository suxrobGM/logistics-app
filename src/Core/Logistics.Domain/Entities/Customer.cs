using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// Company's customer (e.g. broker, shipper, etc.).
/// </summary>
public class Customer : AuditableEntity, ITenantEntity
{
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Address? Address { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public string? Notes { get; set; }

    /// <summary>
    /// VAT ID (EU), EIN (US), GSTIN (IN), ABN (AU), etc. Required for EU B2B reverse-charge.
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// When true, the tax calculator skips computing tax for this customer (charity, gov, NGO).
    /// </summary>
    public bool IsVatExempt { get; set; }

    /// <summary>
    /// Detention free time agreed with this customer. Null falls back to the tenant's.
    /// </summary>
    public int? DetentionFreeMinutes { get; set; }

    /// <summary>
    /// Hourly detention rate agreed with this customer. Null falls back to the tenant's.
    /// </summary>
    public decimal? DetentionRatePerHour { get; set; }

    public virtual List<LoadInvoice> Invoices { get; set; } = [];
}
