using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

/// <summary>
/// FMCSA standard inspection categories for commercial motor vehicles
/// </summary>
public enum DvirInspectionCategory
{
    [Description("Air Compressor")] [EnumMember(Value = "air_compressor")]
    AirCompressor,

    [Description("Air Lines")] [EnumMember(Value = "air_lines")]
    AirLines,

    [Description("Battery")] [EnumMember(Value = "battery")]
    Battery,

    [Description("Brakes - Service")] [EnumMember(Value = "brakes_service")]
    BrakesService,

    [Description("Brakes - Parking")] [EnumMember(Value = "brakes_parking")]
    BrakesParking,

    [Description("Clutch")] [EnumMember(Value = "clutch")]
    Clutch,

    [Description("Coupling Devices")] [EnumMember(Value = "coupling_devices")]
    CouplingDevices,

    [Description("Defroster/Heater")] [EnumMember(Value = "defroster_heater")]
    DefrosterHeater,

    [Description("Drive Line")] [EnumMember(Value = "drive_line")]
    DriveLine,

    [Description("Engine")] [EnumMember(Value = "engine")]
    Engine,

    [Description("Exhaust System")] [EnumMember(Value = "exhaust")]
    Exhaust,

    [Description("Fifth Wheel")] [EnumMember(Value = "fifth_wheel")]
    FifthWheel,

    [Description("Fluid Levels")] [EnumMember(Value = "fluid_levels")]
    FluidLevels,

    [Description("Frame and Assembly")] [EnumMember(Value = "frame")]
    Frame,

    [Description("Front Axle")] [EnumMember(Value = "front_axle")]
    FrontAxle,

    [Description("Fuel System")] [EnumMember(Value = "fuel_system")]
    FuelSystem,

    [Description("Horn")] [EnumMember(Value = "horn")]
    Horn,

    [Description("Lights - Head")] [EnumMember(Value = "lights_head")]
    LightsHead,

    [Description("Lights - Tail")] [EnumMember(Value = "lights_tail")]
    LightsTail,

    [Description("Lights - Brake")] [EnumMember(Value = "lights_brake")]
    LightsBrake,

    [Description("Lights - Turn Signals")] [EnumMember(Value = "lights_turn")]
    LightsTurn,

    [Description("Lights - Clearance/Marker")] [EnumMember(Value = "lights_marker")]
    LightsMarker,

    [Description("Mirrors")] [EnumMember(Value = "mirrors")]
    Mirrors,

    [Description("Muffler")] [EnumMember(Value = "muffler")]
    Muffler,

    [Description("Oil Pressure")] [EnumMember(Value = "oil_pressure")]
    OilPressure,

    [Description("Radiator")] [EnumMember(Value = "radiator")]
    Radiator,

    [Description("Rear End")] [EnumMember(Value = "rear_end")]
    RearEnd,

    [Description("Reflectors")] [EnumMember(Value = "reflectors")]
    Reflectors,

    [Description("Safety Equipment")] [EnumMember(Value = "safety_equipment")]
    SafetyEquipment,

    [Description("Seat Belts")] [EnumMember(Value = "seat_belts")]
    SeatBelts,

    [Description("Speedometer")] [EnumMember(Value = "speedometer")]
    Speedometer,

    [Description("Springs")] [EnumMember(Value = "springs")]
    Springs,

    [Description("Starter")] [EnumMember(Value = "starter")]
    Starter,

    [Description("Steering")] [EnumMember(Value = "steering")]
    Steering,

    [Description("Suspension")] [EnumMember(Value = "suspension")]
    Suspension,

    [Description("Tires")] [EnumMember(Value = "tires")]
    Tires,

    [Description("Transmission")] [EnumMember(Value = "transmission")]
    Transmission,

    [Description("Trip Recorder/ELD")] [EnumMember(Value = "trip_recorder")]
    TripRecorder,

    [Description("Wheels and Rims")] [EnumMember(Value = "wheels")]
    Wheels,

    [Description("Windows")] [EnumMember(Value = "windows")]
    Windows,

    [Description("Windshield")] [EnumMember(Value = "windshield")]
    Windshield,

    [Description("Windshield Wipers")] [EnumMember(Value = "wipers")]
    Wipers,

    [Description("Other")] [EnumMember(Value = "other")]
    Other
}
