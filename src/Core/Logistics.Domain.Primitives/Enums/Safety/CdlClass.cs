using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum CdlClass
{
    [Description("Class A - Combination vehicles 26,001+ lbs")] [EnumMember(Value = "class_a")]
    ClassA,

    [Description("Class B - Single vehicles 26,001+ lbs")] [EnumMember(Value = "class_b")]
    ClassB,

    [Description("Class C - Vehicles under 26,001 lbs with hazmat or 16+ passengers")] [EnumMember(Value = "class_c")]
    ClassC
}
