using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class BulkDispatchLoadsValidator : AbstractValidator<BulkDispatchLoadsCommand>
{
    public BulkDispatchLoadsValidator()
    {
        RuleFor(x => x.LoadIds)
            .NotEmpty()
            .WithMessage("At least one load ID is required");

        RuleFor(x => x.LoadIds)
            .Must(ids => ids.Length <= 100)
            .WithMessage("Cannot dispatch more than 100 loads at once");
    }
}
