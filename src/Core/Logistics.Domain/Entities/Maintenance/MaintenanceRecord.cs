using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Maintenance;

namespace Logistics.Domain.Entities.Maintenance;

/// <summary>
/// Service/maintenance record for a truck
/// </summary>
public class MaintenanceRecord : AuditableEntity, ITenantEntity
{
    public Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    /// <summary>
    /// Link to schedule if this was scheduled maintenance
    /// </summary>
    public Guid? MaintenanceScheduleId { get; set; }
    public virtual MaintenanceSchedule? MaintenanceSchedule { get; set; }

    public required MaintenanceType MaintenanceType { get; set; }
    public required DateTime ServiceDate { get; set; }
    public required int OdometerReading { get; set; }
    public int? EngineHours { get; set; }

    public string? VendorName { get; set; }
    public string? VendorAddress { get; set; }
    public string? InvoiceNumber { get; set; }

    public decimal LaborCost { get; set; }
    public decimal PartsCost { get; set; }
    public decimal TotalCost { get; set; }

    public string? Description { get; set; }
    public string? WorkPerformed { get; set; }

    /// <summary>
    /// If performed internally
    /// </summary>
    public Guid? PerformedById { get; set; }
    public virtual Employee? PerformedBy { get; set; }

    public virtual List<TruckDocument> Documents { get; set; } = [];
    public virtual List<MaintenancePart> Parts { get; set; } = [];
}
