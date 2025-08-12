using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum DocumentStatus
{
    [Description("Active"), EnumMember(Value = "active")]
    Active,

    [Description("Archived"), EnumMember(Value = "archived")]
    Archived,

    [Description("Deleted"), EnumMember(Value = "deleted")]
    Deleted
}
