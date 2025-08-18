using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CreateTripValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.TruckId).NotEmpty();

        RuleFor(i => i.PlannedStart)
            .Must(dt => dt > DateTime.UtcNow)
            .WithMessage("Planned start time must be in the future.");

        // Either NewLoads OR ExistingLoadIds (but not both), and whichever is present must be non-empty
        RuleFor(x => x)
            .Must(ExactlyOneSourceSelected)
            .WithMessage("Specify either NewLoads or ExistingLoadIds, but not both.");

        When(x => x.NewLoads is not null, () =>
        {
            RuleFor(x => x.NewLoads!)
                .Must(l => l.Any())
                .WithMessage("NewLoads cannot be empty when provided.");
        });

        When(x => x.AttachLoadIds is not null, () =>
        {
            RuleFor(x => x.AttachLoadIds!)
                .Must(l => l.Any())
                .WithMessage("ExistingLoadIds cannot be empty when provided.");
        });
    }

    private static bool ExactlyOneSourceSelected(CreateTripCommand x)
    {
        var hasNew = x.NewLoads?.Any() == true;
        var hasExisting = x.AttachLoadIds?.Any() == true;
        return hasNew ^ hasExisting; // XOR
    }
}
