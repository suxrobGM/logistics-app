using FluentValidation;

namespace Logistics.Application.Modules.Operations.Documents.Commands;

internal sealed class CaptureBillOfLadingValidator : AbstractValidator<CaptureBillOfLadingCommand>
{
    public CaptureBillOfLadingValidator()
    {
        RuleFor(x => x.UnableToSignReason).MaximumLength(500);
    }
}
