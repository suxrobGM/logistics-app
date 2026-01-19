using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum ExpenseStatus
{
    [Description("Pending"), EnumMember(Value = "pending")]
    Pending,

    [Description("Approved"), EnumMember(Value = "approved")]
    Approved,

    [Description("Rejected"), EnumMember(Value = "rejected")]
    Rejected,

    [Description("Paid"), EnumMember(Value = "paid")]
    Paid
}
