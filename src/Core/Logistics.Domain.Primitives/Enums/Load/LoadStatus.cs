using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum LoadStatus
{
    [Description("Draft")] [EnumMember(Value = "draft")]
    Draft, // created/quoted/booked but not dispatched yet

    [Description("Dispatched")] [EnumMember(Value = "dispatched")]
    Dispatched,

    [Description("Picked Up")] [EnumMember(Value = "picked_up")]
    PickedUp,

    [Description("Delivered")] [EnumMember(Value = "delivered")]
    Delivered,

    [Description("Cancelled")] [EnumMember(Value = "cancelled")]
    Cancelled // aborted at any stage before Delivered
}
