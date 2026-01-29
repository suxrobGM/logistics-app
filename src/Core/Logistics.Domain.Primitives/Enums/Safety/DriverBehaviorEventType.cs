using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum DriverBehaviorEventType
{
    [Description("Harsh Braking")] [EnumMember(Value = "harsh_braking")]
    HarshBraking,

    [Description("Harsh Acceleration")] [EnumMember(Value = "harsh_acceleration")]
    HarshAcceleration,

    [Description("Harsh Cornering")] [EnumMember(Value = "harsh_cornering")]
    HarshCornering,

    [Description("Speeding")] [EnumMember(Value = "speeding")]
    Speeding,

    [Description("Distracted Driving")] [EnumMember(Value = "distracted_driving")]
    DistractedDriving,

    [Description("Drowsiness Detected")] [EnumMember(Value = "drowsiness")]
    Drowsiness,

    [Description("Tailgating")] [EnumMember(Value = "tailgating")]
    Tailgating,

    [Description("Rolling Stop")] [EnumMember(Value = "rolling_stop")]
    RollingStop,

    [Description("Cell Phone Use")] [EnumMember(Value = "cell_phone_use")]
    CellPhoneUse,

    [Description("Seatbelt Violation")] [EnumMember(Value = "seatbelt_violation")]
    SeatbeltViolation,

    [Description("Camera Obstruction")] [EnumMember(Value = "camera_obstruction")]
    CameraObstruction,

    [Description("Forward Collision Warning")] [EnumMember(Value = "forward_collision_warning")]
    ForwardCollisionWarning,

    [Description("Lane Departure Warning")] [EnumMember(Value = "lane_departure_warning")]
    LaneDepartureWarning
}
