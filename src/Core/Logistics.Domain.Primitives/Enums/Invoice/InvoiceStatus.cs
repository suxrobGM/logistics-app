using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum InvoiceStatus
{
    [Description("Draft"), EnumMember(Value = "draft")]
    Draft,

    [Description("Issued"), EnumMember(Value = "issued")]
    Issued,

    [Description("Partially Paid"), EnumMember(Value = "partially_paid")]
    PartiallyPaid,

    [Description("Paid"), EnumMember(Value = "paid")]
    Paid,

    [Description("Cancelled"), EnumMember(Value = "cancelled")]
    Cancelled
}
