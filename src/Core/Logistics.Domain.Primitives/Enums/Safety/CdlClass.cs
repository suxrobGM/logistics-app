using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum CdlClass
{
    [Description("Class A - Combination vehicles 26,001+ lbs")]
    ClassA,

    [Description("Class B - Single vehicles 26,001+ lbs")]
    ClassB,

    [Description("Class C - Vehicles under 26,001 lbs with hazmat or 16+ passengers")]
    ClassC
}
