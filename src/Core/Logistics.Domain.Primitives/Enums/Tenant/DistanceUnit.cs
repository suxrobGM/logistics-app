using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum DistanceUnit
{
    [Description("Miles"), EnumMember(Value = "miles")]
    Miles,

    [Description("Kilometers"), EnumMember(Value = "kilometers")]
    Kilometers
}
