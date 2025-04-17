using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum LoadStatus
{
    [Description("Dispatched")] Dispatched,
    [Description("Picked Up")] PickedUp,
    [Description("Delivered")] Delivered
}
