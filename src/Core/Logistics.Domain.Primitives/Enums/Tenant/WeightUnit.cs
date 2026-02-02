using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum WeightUnit
{
    [Description("Pounds"), EnumMember(Value = "pounds")]
    Pounds,

    [Description("Kilograms"), EnumMember(Value = "kilograms")]
    Kilograms
}
