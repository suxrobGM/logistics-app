using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// What went wrong on an active trip, for the AI trip alert the dispatch agent investigates.
/// </summary>
public enum TripAlertKind
{
    RunningLate,

    [Description("Hours shortfall")]
    HosShortfall,

    [Description("HOS violation")]
    HosViolation,

    Detention
}
