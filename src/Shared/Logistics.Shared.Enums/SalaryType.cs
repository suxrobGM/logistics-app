using System.ComponentModel;

namespace Logistics.Shared.Enums;

public enum SalaryType
{
    [Description("Monthly")]
    Monthly,
    
    [Description("Weekly")]
    Weekly,
    
    [Description("Share of gross")]
    ShareOfGross
}
