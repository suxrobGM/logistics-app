using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Payroll;

public class EmployeePayTests
{
    [Theory]
    [InlineData(false, false, 2600, "gross revenue")]
    [InlineData(true, false, 2200, "linehaul and accessorials")]
    [InlineData(false, true, 2400, "linehaul and fuel surcharge")]
    [InlineData(true, true, 2000, "linehaul")]
    public void PayableRevenue_AppliesExclusions(bool excludeFuel, bool excludeAccessorials, int expected, string basis)
    {
        var employee = new Employee
        {
            Email = "d@x.com", FirstName = "Ana", LastName = "Diaz", Salary = Money.Zero("USD"),
            PayExcludesFuelSurcharge = excludeFuel, PayExcludesAccessorials = excludeAccessorials
        };

        Assert.Equal(expected, employee.PayableRevenue(linehaul: 2000m, fuelSurcharge: 400m, accessorials: 200m));
        Assert.Equal(basis, employee.PayableRevenueBasis());
    }
}
