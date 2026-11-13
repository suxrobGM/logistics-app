using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Financial.Payroll;

/// <summary>Payroll's own tests pin the per-type amounts; these pin that each type is paid by exactly one of the two methods.</summary>
public class DriverPayCalculatorTests
{
    private readonly DriverPayCalculator _sut = new();

    private static readonly LoadPayBasis Load = new(1000m, 100m, 50m, 160_934.4, DepartedStops: 2);

    public static TheoryData<SalaryType, decimal, decimal> Cases => new()
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

    [Theory]
    [MemberData(nameof(Cases))]
    public void Pay_EachSalaryType_PaidByLoadsOrByPeriod(SalaryType type, decimal byLoads, decimal byPeriod)
    {
        var driver = TestData.Driver();
        driver.SalaryType = type;
        driver.Salary = new Money { Amount = 1m, Currency = "USD" };

        Assert.Equal(byLoads, _sut.PayForLoads(driver, [Load], DistanceUnit.Miles));
        Assert.Equal(byPeriod, _sut.PayForPeriod(driver, weeks: 2, months: 3, hours: 4));
    }
}
