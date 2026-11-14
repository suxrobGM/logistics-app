using FluentValidation.TestHelper;
using Logistics.Application.Modules.Reporting.Stats.Queries;
using Xunit;

namespace Logistics.Application.Tests.Reporting.Stats;

public class GetDailyGrossesValidatorTests
{
    private static readonly DateTime Now = new(2026, 11, 14, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_FromDateWithoutToDate_Passes()
    {
        var query = new GetDailyGrossesQuery { FromDate = Now.AddDays(-7) };

        new GetDailyGrossesValidator().TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_FromDateAfterToDate_Fails()
    {
        var query = new GetDailyGrossesQuery { FromDate = Now, ToDate = Now.AddDays(-1) };

        new GetDailyGrossesValidator().TestValidate(query).ShouldHaveValidationErrorFor(q => q.FromDate);
    }

    [Fact]
    public void Validate_MissingFromDate_Fails()
    {
        var query = new GetDailyGrossesQuery { ToDate = Now };

        new GetDailyGrossesValidator().TestValidate(query).ShouldHaveValidationErrorFor(q => q.FromDate);
    }
}
