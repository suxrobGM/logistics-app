using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum CompanyExpenseCategory
{
    [Description("Office"), EnumMember(Value = "office")]
    Office,

    [Description("Software"), EnumMember(Value = "software")]
    Software,

    [Description("Insurance"), EnumMember(Value = "insurance")]
    Insurance,

    [Description("Legal"), EnumMember(Value = "legal")]
    Legal,

    [Description("Travel"), EnumMember(Value = "travel")]
    Travel,

    [Description("Other"), EnumMember(Value = "other")]
    Other
}
