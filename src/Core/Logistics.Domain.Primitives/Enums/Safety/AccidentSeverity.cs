using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum AccidentSeverity
{
    [Description("Minor - No injuries, minimal damage")] [EnumMember(Value = "minor")]
    Minor,

    [Description("Moderate - Minor injuries or significant damage")] [EnumMember(Value = "moderate")]
    Moderate,

    [Description("Severe - Serious injuries")] [EnumMember(Value = "severe")]
    Severe,

    [Description("Fatal - Fatalities involved")] [EnumMember(Value = "fatal")]
    Fatal
}
