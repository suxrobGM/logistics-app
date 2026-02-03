using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum DvirType
{
    [Description("Pre-Trip Inspection")]
    PreTrip,

    [Description("Post-Trip Inspection")]
    PostTrip
}
