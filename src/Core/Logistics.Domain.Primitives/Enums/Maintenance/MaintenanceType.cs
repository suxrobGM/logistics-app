using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Maintenance;

public enum MaintenanceType
{
    OilChange,
    TireRotation,
    TireReplacement,
    BrakeInspection,
    BrakeReplacement,
    AirFilterReplacement,
    FuelFilterReplacement,
    TransmissionService,
    CoolantFlush,
    BeltInspection,

    [Description("Battery Check/Replacement")]
    Battery,

    [Description("Annual DOT Inspection")]
    AnnualDotInspection,

    PreventiveMaintenance,
    EngineService,
    SuspensionService,
    ElectricalRepair,
    BodyWork,

    [Description("HVAC Service")]
    HvacService,

    ExhaustSystem,
    SteeringRepair,
    Other
}
