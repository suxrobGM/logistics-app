using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum TripStopType
{
    [Description("PickUp"), EnumMember(Value = "pick_up")]  PickUp,
    [Description("DropOff"), EnumMember(Value = "drop_off")] DropOff,
}
