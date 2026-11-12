using FluentValidation;

namespace Logistics.Application.Modules.Compliance.Accidents.Commands;

internal sealed class UpdateAccidentReportValidator : AbstractValidator<UpdateAccidentReportCommand>
{
    public UpdateAccidentReportValidator()
    {
        RuleFor(c => c.NumberOfFatalities).GreaterThanOrEqualTo(0);
    }
}
