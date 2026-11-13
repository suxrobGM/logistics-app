using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
///     Represents the type of load being transported.
///     Load types can vary based on the nature of the goods, their handling requirements, and the type of vehicle needed
///     for transport.
/// </summary>
public enum LoadType
{
    GeneralFreight,
    RefrigeratedGoods,
    HazardousMaterials,

    [Description("Oversize / Heavy")]
    OversizeHeavy,

    [Description("Liquid / Tanker")]
    Liquid,

    [Description("Bulk (Gravel, Grain)")]
    Bulk,

    [Description("Vehicle / Car")]
    Vehicle,

    Livestock,

    [Description("Intermodal Container")]
    IntermodalContainer,

    [Description("Tank Container")]
    TankContainer,

    [Description("Reefer Container")]
    ReeferContainer,

    [Description("Break Bulk")]
    BreakBulk,

    [Description("High Value")]
    HighValue,

    Pharmaceutical,

    [Description("Project Cargo")]
    ProjectCargo
}
