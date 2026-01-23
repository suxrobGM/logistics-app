using FluentValidation;
using Logistics.Application.Constants;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.Type).IsInEnum();
        RuleFor(i => i.AssignedDispatcherId).NotEmpty();
        // AssignedTruckId is optional - load can be created without truck assignment (e.g., from load board)
        RuleFor(i => i.OriginAddress).NotEmpty();
        RuleFor(i => i.OriginLocation).NotEmpty();
        RuleFor(i => i.DestinationAddress).NotEmpty();
        RuleFor(i => i.DestinationLocation).NotEmpty();
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConstants.MinDeliveryCost)
            .LessThan(LoadConstants.MaxDeliveryCost);
    }
}
