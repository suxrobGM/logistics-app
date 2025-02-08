using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadValidator : AbstractValidator<CreateLoadCommand>
{
    public CreateLoadValidator()
    {
        RuleFor(i => i.AssignedDispatcherId).NotEmpty();
        RuleFor(i => i.AssignedTruckId).NotEmpty();
        RuleFor(i => i.OriginAddress).NotEmpty();
        RuleFor(i => i.OriginAddressLat).NotEmpty().InclusiveBetween(-90, 90);
        RuleFor(i => i.OriginAddressLong).NotEmpty().InclusiveBetween(-180, 180);
        RuleFor(i => i.DestinationAddress).NotEmpty();
        RuleFor(i => i.DestinationAddressLat).NotEmpty().InclusiveBetween(-90, 90);
        RuleFor(i => i.DestinationAddressLong).NotEmpty().InclusiveBetween(-180, 180);
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConsts.MinDeliveryCost)
            .LessThan(LoadConsts.MaxDeliveryCost);
    }
}
