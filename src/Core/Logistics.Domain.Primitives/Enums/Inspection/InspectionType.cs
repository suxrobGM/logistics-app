using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum InspectionType
{
    [Description("Draft"), EnumMember(Value = "pickup")]
    Pickup,
    [Description("Delivery"), EnumMember(Value = "delivery")]
    Delivery
}
