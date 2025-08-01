using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Operational state of a truck at any given moment.
/// </summary>
public enum TruckStatus
{
    [Description("Available"), EnumMember(Value = "available")]
    Available, // ready for dispatch / assignment

    [Description("En Route"), EnumMember(Value = "en_route")]
    EnRoute, // currently travelling with or without a load

    [Description("Loading"), EnumMember(Value = "loading")]
    Loading, // at origin, cargo being loaded

    [Description("Unloading"), EnumMember(Value = "unloading")]
    Unloading, // at destination, cargo being unloaded

    // ───── Non-operational / admin states ──────────────────
    [Description("Maintenance"), EnumMember(Value = "maintenance")]
    Maintenance, // scheduled or unscheduled repairs/inspection

    [Description("Out of Service"), EnumMember(Value = "out_of_service")]
    OutOfService, // breakdown or legal hold — not dispatchable

    [Description("Offline"), EnumMember(Value = "offline")]
    Offline // tracker not reporting / unknown location
}
