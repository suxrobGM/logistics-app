using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum DemoRequestStatus
{
    [Description("New")] [EnumMember(Value = "new")]
    New,

    [Description("Contacted")] [EnumMember(Value = "contacted")]
    Contacted,

    [Description("Converted")] [EnumMember(Value = "converted")]
    Converted,

    [Description("Closed")] [EnumMember(Value = "closed")]
    Closed,
}
