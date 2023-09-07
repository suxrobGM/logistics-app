using FluentValidation;
using Logistics.Domain.Constraints;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    private const string CoordinatesPattern = @"^(-?\d+(\.\d+)?),\s*(-?\d+(\.\d+)?)$";
    
    public CreateLoadValidator()
    {
        RuleFor(i => i.AssignedDispatcherId).NotEmpty();
        RuleFor(i => i.AssignedTruckNumber).NotEmpty();
        RuleFor(i => i.OriginAddress).NotEmpty();
        RuleFor(i => i.OriginCoordinates).NotEmpty().Matches(CoordinatesPattern);
        RuleFor(i => i.DestinationAddress).NotEmpty();
        RuleFor(i => i.DestinationCoordinates).NotEmpty().Matches(CoordinatesPattern);
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConsts.MinDeliveryCost)
            .LessThan(LoadConsts.MaxDeliveryCost);
    }
}
