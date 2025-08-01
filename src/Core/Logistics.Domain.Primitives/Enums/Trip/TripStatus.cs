using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum TripStatus
{
    [Description("Planned"), EnumMember(Value = "planned")]  Planned,        // loads assigned, waiting for dispatch
    [Description("Dispatched"), EnumMember(Value = "dispatched")] Dispatched,     // driver left origin yard
    [Description("In Transit"), EnumMember(Value = "in_transit")] InTransit,      // at least one load picked up, not all delivered
    [Description("Completed"), EnumMember(Value = "completed")] Completed,      // all loads delivered
    [Description("Cancelled"), EnumMember(Value = "cancelled")] Cancelled
}