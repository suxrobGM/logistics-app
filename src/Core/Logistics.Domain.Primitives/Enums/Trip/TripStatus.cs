using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum TripStatus
{
    [Description("Draft")] [EnumMember(Value = "planned")]
    Draft, // loads assigned, not yet dispatched

    [Description("Dispatched")] [EnumMember(Value = "dispatched")]
    Dispatched, // driver left origin yard

    [Description("In Transit")] [EnumMember(Value = "in_transit")]
    InTransit, // at least one load picked up, not all delivered

    [Description("Completed")] [EnumMember(Value = "completed")]
    Completed, // all loads delivered

    [Description("Cancelled")] [EnumMember(Value = "cancelled")]
    Cancelled
}
