using FluentValidation.TestHelper;
using Logistics.Application.Modules.Compliance.Safety.Commands;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.Compliance.Safety;

public class CreateAccidentReportValidatorTests
{
    private readonly CreateAccidentReportValidator _sut = new();

    [Fact]
    public void Validate_NegativeFatalities_Fails()
    {
        var command = Command(-1);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NumberOfFatalities);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(3)]
    public void Validate_NullOrNonNegativeFatalities_Passes(int? fatalities)
    {
        var command = Command(fatalities);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.NumberOfFatalities);
    }

    private static CreateAccidentReportCommand Command(int? fatalities) => new()
    {
        TruckId = Guid.NewGuid(),
        DriverId = Guid.NewGuid(),
        Type = AccidentType.Collision,
        Severity = AccidentSeverity.Minor,
        AccidentDateTime = DateTime.UtcNow,
        Location = "I-35, Dallas, TX",
        Description = "Rear-ended at a light",
        FatalitiesReported = true,
        NumberOfFatalities = fatalities
    };
}
