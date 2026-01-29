using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Maintenance;

public enum MaintenanceIntervalType
{
    [Description("Mileage Based")] [EnumMember(Value = "mileage")]
    Mileage,

    [Description("Time Based")] [EnumMember(Value = "time")]
    TimeBased,

    [Description("Engine Hours")] [EnumMember(Value = "engine_hours")]
    EngineHours,

    [Description("Combined (First to Occur)")] [EnumMember(Value = "combined")]
    Combined
}
