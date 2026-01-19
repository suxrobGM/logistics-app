using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum PostedTruckStatus
{
    [Description("Available")] [EnumMember(Value = "available")]
    Available = 0,

    [Description("Booked")] [EnumMember(Value = "booked")]
    Booked = 1,

    [Description("Hidden")] [EnumMember(Value = "hidden")]
    Hidden = 2,

    [Description("Expired")] [EnumMember(Value = "expired")]
    Expired = 3
}
