using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum TruckType
{
    [Description("Flatbed"), EnumMember(Value = "flatbed")] Flatbed,
    [Description("Dry Van"), EnumMember(Value = "dry_van")] DryVan,
    [Description("Reefer"), EnumMember(Value = "reefer")] Reefer,
    [Description("Tanker"), EnumMember(Value = "tanker")] Tanker,
    [Description("Box Truck"), EnumMember(Value = "box_truck")] BoxTruck,
    [Description("Dump Truck"), EnumMember(Value = "dump_truck")] DumpTruck,
    [Description("Tow Truck"), EnumMember(Value = "tow_truck")] TowTruck,
    [Description("Car Hauler"), EnumMember(Value = "car_hauler")] CarHauler,
}
