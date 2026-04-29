using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateContainerValidator : AbstractValidator<CreateContainerCommand>
{
    public CreateContainerValidator()
    {
        RuleFor(i => i.Number)
            .NotEmpty()
            .Length(11)
            .WithMessage("Container number must be exactly 11 characters (ISO 6346).");

        RuleFor(i => i.GrossWeight).GreaterThanOrEqualTo(0);
    }
}
