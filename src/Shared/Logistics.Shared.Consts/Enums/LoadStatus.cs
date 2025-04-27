using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum LoadStatus
{
    [Description("Dispatched"), EnumMember(Value = "dispatched")] 
    Dispatched,
    
    [Description("Picked Up"), EnumMember(Value = "picked_up")] 
    PickedUp,
    
    [Description("Delivered"), EnumMember(Value = "delivered")] 
    Delivered
}
