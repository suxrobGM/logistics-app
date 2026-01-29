using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums.Maintenance;

namespace Logistics.Domain.Entities.Maintenance;

/// <summary>
/// Recurring maintenance schedule for a truck
/// </summary>
public class MaintenanceSchedule : AuditableEntity, ITenantEntity
{
    public Guid TruckId { get; set; }
    public virtual Truck Truck { get; set; } = null!;

    public required MaintenanceType MaintenanceType { get; set; }
    public required MaintenanceIntervalType IntervalType { get; set; }

    /// <summary>
    /// Interval in miles (for mileage-based)
    /// </summary>
    public int? MileageInterval { get; set; }

    /// <summary>
    /// Interval in days (for time-based)
    /// </summary>
    public int? DaysInterval { get; set; }

    /// <summary>
    /// Interval in engine hours
    /// </summary>
    public int? EngineHoursInterval { get; set; }

    // Last service tracking
    public int? LastServiceMileage { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public int? LastServiceEngineHours { get; set; }

    // Next due calculations
    public int? NextDueMileage { get; set; }
    public DateTime? NextDueDate { get; set; }
    public int? NextDueEngineHours { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public bool IsOverdue => NextDueDate.HasValue && NextDueDate < DateTime.UtcNow;
    public bool IsDueSoon => !IsOverdue && NextDueDate.HasValue && NextDueDate <= DateTime.UtcNow.AddDays(7);
}
