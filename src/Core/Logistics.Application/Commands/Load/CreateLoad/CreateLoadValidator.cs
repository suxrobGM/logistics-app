using FluentValidation;
using Logistics.Application.Constants;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.Type).NotEmpty().IsInEnum();
        RuleFor(i => i.AssignedDispatcherId).NotEmpty();
        RuleFor(i => i.AssignedTruckId).NotEmpty();
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
