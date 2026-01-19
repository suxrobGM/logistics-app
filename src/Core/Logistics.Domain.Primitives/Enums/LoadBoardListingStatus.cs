using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum LoadBoardListingStatus
{
    [Description("Available")] [EnumMember(Value = "available")]
    Available = 0,

    [Description("Booked")] [EnumMember(Value = "booked")]
    Booked = 1,

    [Description("In Transit")] [EnumMember(Value = "in_transit")]
    InTransit = 2,

    [Description("Completed")] [EnumMember(Value = "completed")]
    Completed = 3,

    [Description("Cancelled")] [EnumMember(Value = "cancelled")]
    Cancelled = 4,

    [Description("Expired")] [EnumMember(Value = "expired")]
    Expired = 5
}
