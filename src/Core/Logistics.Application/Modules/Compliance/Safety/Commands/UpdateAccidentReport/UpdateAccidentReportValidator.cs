using FluentValidation;

namespace Logistics.Application.Modules.Compliance.Safety.Commands;

internal sealed class UpdateAccidentReportValidator : AbstractValidator<UpdateAccidentReportCommand>
{
    public UpdateAccidentReportValidator()
    {
        RuleFor(c => c.NumberOfFatalities).GreaterThanOrEqualTo(0);
    }
}
