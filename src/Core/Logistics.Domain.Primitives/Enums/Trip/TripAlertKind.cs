using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum TripAlertKind
{
    RunningLate,

    [Description("Hours shortfall")]
    HosShortfall,

    [Description("HOS violation")]
    HosViolation,

    Detention
}
