using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class AddLoadToTripValidator : AbstractValidator<AddLoadToTripCommand>
{
    public AddLoadToTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();

        RuleFor(x => x)
            .Must(ExactlyOneSourceSelected)
            .WithMessage("Specify either NewLoad or ExistingLoadId, but not both.");

        When(x => x.NewLoad is not null, () =>
        {
            RuleFor(x => x.NewLoad)
                .Must(l => l is not null)
                .WithMessage("NewLoad cannot be empty when provided.");
        });

        When(x => x.ExistingLoadId is not null, () =>
        {
            RuleFor(x => x.ExistingLoadId)
                .Must(l => l is not null)
                .WithMessage("ExistingLoadId cannot be empty when provided.");
        });
    }

    private static bool ExactlyOneSourceSelected(AddLoadToTripCommand x)
    {
        var hasNew = x.NewLoad is not null;
        var hasExisting = x.ExistingLoadId is not null;
        return hasNew ^ hasExisting; // XOR
    }
}
