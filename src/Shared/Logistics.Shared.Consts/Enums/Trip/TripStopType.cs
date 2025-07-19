using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum TripStopType
{
    [Description("Pick Up"), EnumMember(Value = "pick_up")] PickUp,
    [Description("Drop Off"), EnumMember(Value = "drop_off")] DropOff,
}
