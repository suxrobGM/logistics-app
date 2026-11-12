using FluentValidation;

namespace Logistics.Application.Modules.Compliance.Accidents.Commands;

internal sealed class CreateAccidentReportValidator : AbstractValidator<CreateAccidentReportCommand>
{
    public CreateAccidentReportValidator()
    {
        RuleFor(c => c.NumberOfFatalities).GreaterThanOrEqualTo(0);
    }
}
