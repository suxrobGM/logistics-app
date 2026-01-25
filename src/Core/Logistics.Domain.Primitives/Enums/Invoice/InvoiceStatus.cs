using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum InvoiceStatus
{
    [Description("Draft"), EnumMember(Value = "draft")]
    Draft,

    [Description("Pending Approval"), EnumMember(Value = "pending_approval")]
    PendingApproval,

    [Description("Approved"), EnumMember(Value = "approved")]
    Approved,

    [Description("Rejected"), EnumMember(Value = "rejected")]
    Rejected,

    [Description("Issued"), EnumMember(Value = "issued")]
    Issued,

    [Description("Sent"), EnumMember(Value = "sent")]
    Sent,

    [Description("Partially Paid"), EnumMember(Value = "partially_paid")]
    PartiallyPaid,

    [Description("Paid"), EnumMember(Value = "paid")]
    Paid,

    [Description("Overdue"), EnumMember(Value = "overdue")]
    Overdue,

    [Description("Cancelled"), EnumMember(Value = "cancelled")]
    Cancelled
}
