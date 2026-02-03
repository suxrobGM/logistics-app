using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

/// <summary>
/// FMCSA standard inspection categories for commercial motor vehicles
/// </summary>
public enum DvirInspectionCategory
{
    AirCompressor,
    AirLines,
    Battery,

    [Description("Brakes - Service")]
    BrakesService,

    [Description("Brakes - Parking")]
    BrakesParking,

    Clutch,
    CouplingDevices,

    [Description("Defroster/Heater")]
    DefrosterHeater,

    DriveLine,
    Engine,
    Exhaust,
    FifthWheel,
    FluidLevels,

    [Description("Frame and Assembly")]
    Frame,

    FrontAxle,
    FuelSystem,
    Horn,

    [Description("Lights - Head")]
    LightsHead,

    [Description("Lights - Tail")]
    LightsTail,

    [Description("Lights - Brake")]
    LightsBrake,

    [Description("Lights - Turn Signals")]
    LightsTurn,

    [Description("Lights - Clearance/Marker")]
    LightsMarker,

    Mirrors,
    Muffler,
    OilPressure,
    Radiator,
    RearEnd,
    Reflectors,
    SafetyEquipment,
    SeatBelts,
    Speedometer,
    Springs,
    Starter,
    Steering,
    Suspension,
    Tires,
    Transmission,

    [Description("Trip Recorder/ELD")]
    TripRecorder,

    [Description("Wheels and Rims")]
    Wheels,

    Windows,
    Windshield,
    WindshieldWipers,
    Other
}
