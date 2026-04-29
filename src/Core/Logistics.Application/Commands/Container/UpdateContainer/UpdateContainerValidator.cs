using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateContainerValidator : AbstractValidator<UpdateContainerCommand>
{
    public UpdateContainerValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        When(i => !string.IsNullOrEmpty(i.Number), () =>
        {
            RuleFor(i => i.Number)
                .Length(11)
                .WithMessage("Container number must be exactly 11 characters (ISO 6346).");
        });
        When(i => i.GrossWeight.HasValue, () =>
        {
            RuleFor(i => i.GrossWeight!.Value).GreaterThanOrEqualTo(0);
        });
    }
}
