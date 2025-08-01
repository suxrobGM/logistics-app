using FluentValidation;
using Logistics.Application.Constants;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class UpdateLoadValidator : AbstractValidator<UpdateLoadCommand>
{
    public UpdateLoadValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
        RuleFor(i => i.Distance).GreaterThan(0);
        RuleFor(i => i.DeliveryCost)
            .GreaterThan(LoadConstants.MinDeliveryCost)
            .LessThan(LoadConstants.MaxDeliveryCost);
        
        When(i => i.OriginAddress != null, () =>
        {
            RuleFor(i => i.OriginAddressLat).NotEmpty().InclusiveBetween(-90, 90);
            RuleFor(i => i.OriginAddressLong).NotEmpty().InclusiveBetween(-180, 180);
        });
        
        When(i => i.DestinationAddress != null, () =>
        {
            RuleFor(i => i.DestinationAddressLat).NotEmpty().InclusiveBetween(-90, 90);
            RuleFor(i => i.DestinationAddressLong).NotEmpty().InclusiveBetween(-180, 180);
        });
    }
}
