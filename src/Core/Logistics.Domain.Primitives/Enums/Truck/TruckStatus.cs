using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Operational state of a truck at any given moment.
/// </summary>
public enum TruckStatus
{
    Available, // ready for dispatch / assignment

    [Description("En Route")]
    EnRoute, // currently travelling with or without a load

    Loading, // at origin, cargo being loaded
    Unloading, // at destination, cargo being unloaded

    // ───── Non-operational / admin states ──────────────────
    Maintenance, // scheduled or unscheduled repairs/inspection
    OutOfService, // breakdown or legal hold — not dispatchable
    Offline // tracker not reporting / unknown location
}
