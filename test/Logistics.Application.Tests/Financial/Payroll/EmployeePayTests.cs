using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Financial.Payroll;

public class EmployeePayTests
{
    private static readonly LoadPayBasis Load = new(1000m, 100m, 50m, 100_000, DepartedStops: 2);

    [Theory]
    [InlineData(false, false, 2600, "gross revenue")]
    [InlineData(true, false, 2200, "linehaul and accessorials")]
    [InlineData(false, true, 2400, "linehaul and fuel surcharge")]
    [InlineData(true, true, 2000, "linehaul")]
    public void PayableRevenue_AppliesExclusions(bool excludeFuel, bool excludeAccessorials, int expected, string basis)
    {
        var employee = new Employee
        {
            Email = "d@x.com",
            FirstName = "Ana",
            LastName = "Diaz",
            Salary = 0m,
            PayExcludesFuelSurcharge = excludeFuel,
            PayExcludesAccessorials = excludeAccessorials
        };

        Assert.Equal(expected, employee.PayableRevenue(linehaul: 2000m, fuelSurcharge: 400m, accessorials: 200m));
        Assert.Equal(basis, employee.PayableRevenueBasis());
    }

    public static TheoryData<SalaryType, decimal, decimal> PayCases => new()
    {
        { SalaryType.ShareOfGross, 1150m, 0m },
        { SalaryType.RatePerDistance, 100m, 0m },
        { SalaryType.PerLoad, 1m, 0m },
        { SalaryType.PerStop, 2m, 0m },
        { SalaryType.Weekly, 0m, 2m },
        { SalaryType.Monthly, 0m, 3m },
        { SalaryType.Hourly, 0m, 4m },
        { SalaryType.None, 0m, 1m },
    };

    /// <summary>Payroll's own tests pin the amounts; this pins that each type is paid by exactly one of the two methods.</summary>
    [Theory]
    [MemberData(nameof(PayCases))]
    public void Pay_EachSalaryType_PaidByLoadsOrByPeriod(SalaryType type, decimal byLoads, decimal byPeriod)
    {
        var driver = TestData.Driver();
        driver.SalaryType = type;
        driver.Salary = 1m;

        Assert.Equal(byLoads, driver.PayForLoads([Load]));
        Assert.Equal(byPeriod, driver.PayForPeriod(weeks: 2, months: 3, hours: 4));
    }
}
