using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum AccidentSeverity
{
    [Description("Minor - No injuries, minimal damage")]
    Minor,

    [Description("Moderate - Minor injuries or significant damage")]
    Moderate,

    [Description("Severe - Serious injuries")]
    Severe,

    [Description("Fatal - Fatalities involved")]
    Fatal
}
