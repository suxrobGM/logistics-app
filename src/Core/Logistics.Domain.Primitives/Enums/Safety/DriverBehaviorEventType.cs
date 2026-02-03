using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum DriverBehaviorEventType
{
    HarshBraking,
    HarshAcceleration,
    HarshCornering,
    Speeding,
    DistractedDriving,

    [Description("Drowsiness Detected")]
    Drowsiness,

    Tailgating,
    RollingStop,
    CellPhoneUse,
    SeatbeltViolation,
    CameraObstruction,
    ForwardCollisionWarning,
    LaneDepartureWarning
}
