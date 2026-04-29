using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum TruckType
{
    Flatbed,
    FreightTruck,
    Reefer,
    Tanker,
    BoxTruck,
    DumpTruck,
    TowTruck,
    CarHauler,
    ContainerTruck,
    Tautliner,

    [Description("Low Loader")]
    LowLoader,

    [Description("Car Transporter")]
    CarTransporter,

    [Description("Swap Body")]
    SwapBody,

    Curtainsider
}
