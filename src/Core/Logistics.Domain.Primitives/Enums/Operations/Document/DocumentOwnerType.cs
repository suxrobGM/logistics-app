using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
///     Document owner type.
/// </summary>
public enum DocumentOwnerType
{
    [Description("Load Document")]
    Load,

    [Description("Employee Document")]
    Employee,

    [Description("Delivery Document")]
    Delivery,

    [Description("Truck Document")]
    Truck
}
