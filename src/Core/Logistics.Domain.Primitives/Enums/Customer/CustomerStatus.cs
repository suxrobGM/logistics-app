using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum CustomerStatus
{
    [Description("Active"), EnumMember(Value = "active")]
    Active,

    [Description("Inactive"), EnumMember(Value = "inactive")]
    Inactive,

    [Description("Prospect"), EnumMember(Value = "prospect")]
    Prospect
}
