using FluentValidation;
using Logistics.Application.Constants;

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

        When(i => i.RequestedPickupDate.HasValue && i.RequestedDeliveryDate.HasValue, () =>
        {
            RuleFor(i => i.RequestedDeliveryDate!.Value)
                .GreaterThanOrEqualTo(i => i.RequestedPickupDate!.Value)
                .WithMessage("Requested delivery date must be on or after requested pickup date.");
        });
    }
}
