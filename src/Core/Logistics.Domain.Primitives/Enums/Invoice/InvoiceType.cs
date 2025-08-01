using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum InvoiceType
{
    [Description("Load"), EnumMember(Value = "load")] 
    Load,
    
    [Description("Subscription"), EnumMember(Value = "subscription")]
    Subscription,
    
    [Description("Payroll"), EnumMember(Value = "payroll")]
    Payroll,
    
    [Description("Other"), EnumMember(Value = "other")]
    Other
}