using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum PaymentStatus
{
    [Description("Pending"), EnumMember(Value = "pending")]
    Pending,
    
    [Description("Paid"), EnumMember(Value = "paid")]
    Paid,
    
    [Description("Failed"), EnumMember(Value = "failed")]
    Failed,
    
    [Description("Cancelled"), EnumMember(Value = "cancelled")]
    Cancelled,
    
    [Description("Refunded"), EnumMember(Value = "refunded")]
    Refunded,
    
    [Description("Partially Refunded"), EnumMember(Value = "partially_refunded")]
    PartiallyRefunded,
}
