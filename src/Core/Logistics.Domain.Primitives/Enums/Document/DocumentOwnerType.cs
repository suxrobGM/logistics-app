using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
///     Document owner type.
/// </summary>
public enum DocumentOwnerType
{
    [Description("Load Document")] [EnumMember(Value = "load")]
    Load,

    [Description("Employee Document")] [EnumMember(Value = "employee")]
    Employee,

    [Description("Delivery Document")] [EnumMember(Value = "delivery")]
    Delivery,

    [Description("Truck Document")] [EnumMember(Value = "truck")]
    Truck
}
