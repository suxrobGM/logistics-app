using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum TruckType
{
    [Description("Flatbed"), EnumMember(Value = "flatbed")] Flatbed,
    [Description("Freight Truck"), EnumMember(Value = "freight_truck")] FreightTruck,
    [Description("Reefer"), EnumMember(Value = "reefer")] Reefer,
    [Description("Tanker"), EnumMember(Value = "tanker")] Tanker,
    [Description("Box Truck"), EnumMember(Value = "box_truck")] BoxTruck,
    [Description("Dump Truck"), EnumMember(Value = "dump_truck")] DumpTruck,
    [Description("Tow Truck"), EnumMember(Value = "tow_truck")] TowTruck,
    [Description("Car Hauler"), EnumMember(Value = "car_hauler")] CarHauler,
}
