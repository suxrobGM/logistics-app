using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Represents the type of load being transported.
/// Load types can vary based on the nature of the goods, their handling requirements, and the type of vehicle needed for transport.
/// </summary>
public enum LoadType
{
    [Description("General Freight"), EnumMember(Value = "general_freight")]
    GeneralFreight,

    [Description("Refrigerated Goods"), EnumMember(Value = "refrigerated_goods")]
    RefrigeratedGoods,

    [Description("Hazardous Materials"), EnumMember(Value = "hazardous_materials")]
    HazardousMaterials,

    [Description("Oversize / Heavy"), EnumMember(Value = "oversize_heavy")]
    OversizeHeavy,

    [Description("Liquid / Tanker"), EnumMember(Value = "liquid")]
    Liquid,

    [Description("Bulk (Gravel, Grain)"), EnumMember(Value = "bulk")]
    Bulk,

    [Description("Vehicle / Car"), EnumMember(Value = "vehicle")]
    Vehicle,

    [Description("Livestock"), EnumMember(Value = "livestock")]
    Livestock
}
