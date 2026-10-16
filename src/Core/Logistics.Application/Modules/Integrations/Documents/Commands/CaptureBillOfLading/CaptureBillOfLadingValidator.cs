using FluentValidation;

namespace Logistics.Application.Modules.Integrations.Documents.Commands;

internal sealed class CaptureBillOfLadingValidator : AbstractValidator<CaptureBillOfLadingCommand>
{
    public CaptureBillOfLadingValidator()
    {
        RuleFor(x => x.UnableToSignReason).MaximumLength(500);
    }
}
