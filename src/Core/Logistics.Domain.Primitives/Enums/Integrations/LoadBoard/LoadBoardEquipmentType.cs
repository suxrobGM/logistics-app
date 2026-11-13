using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

/// <summary>
/// Trailer equipment as load boards classify it. The one list the UI offers and every provider
/// maps to its own codes (DAT: <c>DatEquipment</c>).
/// </summary>
public enum LoadBoardEquipmentType
{
    Van,
    Reefer,
    Flatbed,

    [Description("Step Deck")]
    StepDeck,

    [Description("Double Drop")]
    DoubleDrop,

    Lowboy,

    [Description("Removable Gooseneck")]
    RemovableGooseneck,

    Conestoga,
    Hotshot,

    [Description("Power Only")]
    PowerOnly,

    [Description("Auto Carrier")]
    AutoCarrier,

    Container,
    Tanker,

    [Description("Hopper Bottom")]
    HopperBottom,

    [Description("Dump Trailer")]
    DumpTrailer,

    [Description("Box Truck")]
    BoxTruck,

    [Description("Flatbed or Step Deck")]
    FlatbedOrStepDeck
}
