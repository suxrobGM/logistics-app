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
    Hourly,

    /// <summary>Flat amount (Employee.Salary) per load delivered in the period.</summary>
    PerLoad,

    /// <summary>Flat amount (Employee.Salary) per trip stop the driver departed in the period.</summary>
    PerStop
}
