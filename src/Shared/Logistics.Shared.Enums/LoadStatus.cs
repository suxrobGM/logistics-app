using System.ComponentModel;

namespace Logistics.Shared.Enums;

public enum LoadStatus
{
    [Description("Dispatched")]
    Dispatched,
    
    [Description("Picked Up")]
    PickedUp,
    
    [Description("Delivered")]
    Delivered
}
