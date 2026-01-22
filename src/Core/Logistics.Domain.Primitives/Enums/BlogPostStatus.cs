using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum BlogPostStatus
{
    [Description("Draft")] [EnumMember(Value = "draft")]
    Draft,

    [Description("Published")] [EnumMember(Value = "published")]
    Published,

    [Description("Archived")] [EnumMember(Value = "archived")]
    Archived,
}
