using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum PaymentStatus
{
    [Description("Pending"), EnumMember(Value = "pending")]
    Pending,
    
    [Description("Paid"), EnumMember(Value = "paid")]
    Paid,
    
    [Description("Failed"), EnumMember(Value = "failed")]
    Failed,
    
    [Description("Cancelled"), EnumMember(Value = "cancelled")]
    Cancelled
}
