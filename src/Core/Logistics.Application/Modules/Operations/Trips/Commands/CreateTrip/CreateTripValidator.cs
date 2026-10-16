using FluentValidation;
using Logistics.Application.Modules.Operations.Loads.Commands;
using Logistics.Application.Validators;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

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

            RuleForEach(x => x.NewLoads!)
                .ChildRules(load =>
                {
                    load.Include(new LoadChargesValidator());
                    load.RuleForEach(l => l.Vehicles).SetValidator(new LoadVehicleInputValidator());
                    load.RuleFor(l => l.Vehicles)
                        .Empty()
                        .When(l => l.Type != LoadType.Vehicle)
                        .WithMessage("Only vehicle loads can list vehicles.");
                });
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
