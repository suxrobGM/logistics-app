using FluentValidation;
using Logistics.Application.Modules.Operations.Loads.Constants;
using Logistics.Application.Validators;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadValidator()
    {
        Include(new LoadChargesValidator());
        Include(new LoadScheduleValidator());
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.Type).IsInEnum();
        RuleFor(i => i.AssignedDispatcherId).NotEmpty();
        // AssignedTruckId is optional - load can be created without truck assignment (e.g., from load board)
        RuleFor(i => i.OriginAddress).NotNull().SetValidator(new AddressValidator());
        RuleFor(i => i.OriginLocation).NotEmpty();
        RuleFor(i => i.DestinationAddress).NotNull().SetValidator(new AddressValidator());
        RuleFor(i => i.DestinationLocation).NotEmpty();
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleForEach(i => i.Vehicles).SetValidator(new LoadVehicleInputValidator());
        RuleFor(i => i.Vehicles)
            .Empty()
            .When(i => i.Type != LoadType.Vehicle)
            .WithMessage("Only vehicle loads can list vehicles.");
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConstants.MinDeliveryCost)
            .LessThan(LoadConstants.MaxDeliveryCost);
    }
}
