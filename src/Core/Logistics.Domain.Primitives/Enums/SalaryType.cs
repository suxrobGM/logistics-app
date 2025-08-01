using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum SalaryType
{
    [Description("None"), EnumMember(Value = "none")]
    None,
    
    [Description("Monthly"), EnumMember(Value = "monthly")] 
    Monthly,
    
    [Description("Weekly"), EnumMember(Value = "weekly")] 
    Weekly,
    
    [Description("Share of gross"), EnumMember(Value = "share_of_gross")] 
    ShareOfGross
}
