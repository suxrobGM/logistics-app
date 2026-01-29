using Logistics.Domain.Core;

namespace Logistics.Domain.Entities.Maintenance;

/// <summary>
/// Parts used in a maintenance service
/// </summary>
public class MaintenancePart : Entity, ITenantEntity
{
    public Guid MaintenanceRecordId { get; set; }
    public virtual MaintenanceRecord MaintenanceRecord { get; set; } = null!;

    public required string PartName { get; set; }
    public string? PartNumber { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
}
