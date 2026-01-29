using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum DvirType
{
    [Description("Pre-Trip Inspection")] [EnumMember(Value = "pre_trip")]
    PreTrip,

    [Description("Post-Trip Inspection")] [EnumMember(Value = "post_trip")]
    PostTrip
}
