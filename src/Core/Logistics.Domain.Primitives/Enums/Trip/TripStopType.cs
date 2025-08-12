using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum TripStopType
{
    [Description("Pick Up"), EnumMember(Value = "pick_up")] PickUp,
    [Description("Drop Off"), EnumMember(Value = "drop_off")] DropOff,
}
