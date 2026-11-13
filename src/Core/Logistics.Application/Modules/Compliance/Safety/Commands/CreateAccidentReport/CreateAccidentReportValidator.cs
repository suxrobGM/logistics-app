using FluentValidation;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

internal sealed class CreateAccidentReportValidator : AbstractValidator<CreateAccidentReportCommand>
{
    public CreateAccidentReportValidator()
    {
        RuleFor(c => c.NumberOfFatalities).GreaterThanOrEqualTo(0);
    }
}
