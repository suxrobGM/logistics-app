using FluentValidation;
using Logistics.Domain.Constraints;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadValidator : AbstractValidator<UpdateLoadCommand>
{
    private const string CoordinatesPattern = @"^(-?\d+(\.\d+)?),\s*(-?\d+(\.\d+)?)$";
    
    public UpdateLoadValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.OriginCoordinates).Matches(CoordinatesPattern);
        RuleFor(i => i.DestinationCoordinates).Matches(CoordinatesPattern);
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConsts.MinDeliveryCost)
            .LessThan(LoadConsts.MaxDeliveryCost);
    }
}
