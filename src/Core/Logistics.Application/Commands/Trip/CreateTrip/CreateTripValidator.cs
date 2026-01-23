using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateTripValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
        // TruckId is optional - trip can be created without truck assignment (e.g., from load board)

        // Either NewLoads OR AttachedLoadIds (but not both), and whichever is present must be non-empty
        RuleFor(x => x)
            .Must(ExactlyOneSourceSelected)
            .WithMessage("Specify either NewLoads or AttachedLoadIds, but not both.");

        When(x => x.NewLoads is not null, () =>
        {
            RuleFor(x => x.NewLoads!)
                .Must(l => l.Any())
                .WithMessage("NewLoads cannot be empty when provided.");
        });

        When(x => x.AttachedLoadIds is not null, () =>
        {
            RuleFor(x => x.AttachedLoadIds!)
                .Must(l => l.Any())
                .WithMessage("AttachedLoadIds cannot be empty when provided.");
        });
    }

    private static bool ExactlyOneSourceSelected(CreateTripCommand x)
    {
        var hasNew = x.NewLoads?.Any() == true;
        var hasExisting = x.AttachedLoadIds?.Any() == true;
        return hasNew ^ hasExisting; // XOR
    }
}
