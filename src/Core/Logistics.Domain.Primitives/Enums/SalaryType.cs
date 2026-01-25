using System.ComponentModel;
using System.Runtime.Serialization;

namespace Logistics.Domain.Primitives.Enums;

public enum SalaryType
{
    [Description("None")] [EnumMember(Value = "none")]
    None,

    [Description("Monthly")] [EnumMember(Value = "monthly")]
    Monthly,

    [Description("Weekly")] [EnumMember(Value = "weekly")]
    Weekly,

    [Description("Share of gross")] [EnumMember(Value = "share_of_gross")]
    ShareOfGross,

    /// <summary>
    /// For drivers who get paid per distance unit driven.
    /// Rate stored in Employee.Salary, distance unit determined by tenant settings.
    /// </summary>
    [Description("Rate per distance")] [EnumMember(Value = "rate_per_distance")]
    RatePerDistance,

    [Description("Rate per hour")] [EnumMember(Value = "hourly")]
    Hourly
}
