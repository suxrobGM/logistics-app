using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Maintenance;

public enum MaintenanceIntervalType
{
    [Description("Mileage Based")]
    Mileage,

    TimeBased,
    EngineHours,

    [Description("Combined (First to Occur)")]
    Combined
}
