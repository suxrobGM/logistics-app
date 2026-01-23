using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum ContactSubject
{
    [Description("General")] [EnumMember(Value = "general")]
    General,

    [Description("Sales")] [EnumMember(Value = "sales")]
    Sales,

    [Description("Support")] [EnumMember(Value = "support")]
    Support,

    [Description("Partnership")] [EnumMember(Value = "partnership")]
    Partnership,

    [Description("Press")] [EnumMember(Value = "press")]
    Press,
}
