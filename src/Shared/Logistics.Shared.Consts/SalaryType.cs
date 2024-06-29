using System.ComponentModel;

namespace Logistics.Shared.Consts;

public enum SalaryType
{
    [Description("None")]
    None,
    
    [Description("Monthly")]
    Monthly,
    
    [Description("Weekly")]
    Weekly,
    
    [Description("Share of gross")]
    ShareOfGross
}
