using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum TruckExpenseCategory
{
    [Description("Fuel"), EnumMember(Value = "fuel")]
    Fuel,

    [Description("Maintenance"), EnumMember(Value = "maintenance")]
    Maintenance,

    [Description("Tires"), EnumMember(Value = "tires")]
    Tires,

    [Description("Registration"), EnumMember(Value = "registration")]
    Registration,

    [Description("Toll"), EnumMember(Value = "toll")]
    Toll,

    [Description("Parking"), EnumMember(Value = "parking")]
    Parking,

    [Description("Other"), EnumMember(Value = "other")]
    Other
}
