using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Maintenance;

public enum MaintenanceType
{
    [Description("Oil Change")] [EnumMember(Value = "oil_change")]
    OilChange,

    [Description("Tire Rotation")] [EnumMember(Value = "tire_rotation")]
    TireRotation,

    [Description("Tire Replacement")] [EnumMember(Value = "tire_replacement")]
    TireReplacement,

    [Description("Brake Inspection")] [EnumMember(Value = "brake_inspection")]
    BrakeInspection,

    [Description("Brake Replacement")] [EnumMember(Value = "brake_replacement")]
    BrakeReplacement,

    [Description("Air Filter Replacement")] [EnumMember(Value = "air_filter")]
    AirFilterReplacement,

    [Description("Fuel Filter Replacement")] [EnumMember(Value = "fuel_filter")]
    FuelFilterReplacement,

    [Description("Transmission Service")] [EnumMember(Value = "transmission_service")]
    TransmissionService,

    [Description("Coolant Flush")] [EnumMember(Value = "coolant_flush")]
    CoolantFlush,

    [Description("Belt Inspection")] [EnumMember(Value = "belt_inspection")]
    BeltInspection,

    [Description("Battery Check/Replacement")] [EnumMember(Value = "battery")]
    Battery,

    [Description("Annual DOT Inspection")] [EnumMember(Value = "annual_dot")]
    AnnualDotInspection,

    [Description("Preventive Maintenance")] [EnumMember(Value = "preventive")]
    PreventiveMaintenance,

    [Description("Engine Service")] [EnumMember(Value = "engine_service")]
    EngineService,

    [Description("Suspension Service")] [EnumMember(Value = "suspension_service")]
    SuspensionService,

    [Description("Electrical Repair")] [EnumMember(Value = "electrical")]
    ElectricalRepair,

    [Description("Body Work")] [EnumMember(Value = "body_work")]
    BodyWork,

    [Description("HVAC Service")] [EnumMember(Value = "hvac")]
    HvacService,

    [Description("Exhaust System")] [EnumMember(Value = "exhaust")]
    ExhaustSystem,

    [Description("Steering Repair")] [EnumMember(Value = "steering")]
    SteeringRepair,

    [Description("Other")] [EnumMember(Value = "other")]
    Other
}
