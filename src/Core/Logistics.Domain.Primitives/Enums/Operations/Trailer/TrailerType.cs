using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Body type of a trailer, which is what shippers and load boards call the equipment type.
/// </summary>
public enum TrailerType
{
    [Description("Dry Van")]
    DryVan,

    Reefer,
    Flatbed,

    [Description("Step Deck")]
    StepDeck,

    Lowboy,
    Tanker,

    [Description("Car Hauler")]
    CarHauler,

    /// <summary>
    /// Container chassis for intermodal drayage.
    /// </summary>
    Chassis,

    Conestoga
}
