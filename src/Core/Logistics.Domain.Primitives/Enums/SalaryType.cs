using System.ComponentModel;

namespace Logistics.Domain.Primitives.Enums;

public enum SalaryType
{
    None,
    Monthly,
    Weekly,
    ShareOfGross,

    /// <summary>
    /// For drivers who get paid per distance unit driven.
    /// Rate stored in Employee.Salary, distance unit determined by tenant settings.
    /// </summary>
    RatePerDistance,

    [Description("Rate per hour")]
    Hourly
}
