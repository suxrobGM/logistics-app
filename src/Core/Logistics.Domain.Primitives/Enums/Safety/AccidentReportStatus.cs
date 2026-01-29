using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums.Safety;

public enum AccidentReportStatus
{
    [Description("Draft")] [EnumMember(Value = "draft")]
    Draft,

    [Description("Submitted")] [EnumMember(Value = "submitted")]
    Submitted,

    [Description("Under Review")] [EnumMember(Value = "under_review")]
    UnderReview,

    [Description("Pending Documentation")] [EnumMember(Value = "pending_docs")]
    PendingDocumentation,

    [Description("Insurance Filed")] [EnumMember(Value = "insurance_filed")]
    InsuranceFiled,

    [Description("Resolved")] [EnumMember(Value = "resolved")]
    Resolved,

    [Description("Closed")] [EnumMember(Value = "closed")]
    Closed
}
