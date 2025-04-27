using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Shared.Consts;

public enum PaymentFor
{
    [Description("Employee Payroll"), EnumMember(Value = "employee_payroll")]
    Payroll,
    
    [Description("Subscription"), EnumMember(Value = "subscription")]
    Subscription,
    
    [Description("Invoice"), EnumMember(Value = "invoice")]
    Invoice,
    
    [Description("Other"), EnumMember(Value = "other")]
    Other
}
