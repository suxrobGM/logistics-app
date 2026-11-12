using FluentValidation;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class LoadScheduleValidator : AbstractValidator<ILoadScheduleFields>
{
    public LoadScheduleValidator()
    {
        When(i => i.RequestedPickupDate.HasValue && i.RequestedDeliveryDate.HasValue, () =>
        {
            RuleFor(i => i.RequestedDeliveryDate!.Value)
                .GreaterThanOrEqualTo(i => i.RequestedPickupDate!.Value)
                .WithMessage("Requested delivery date must be on or after requested pickup date.");
        });

        When(i => i.PickupAppointmentFrom.HasValue && i.PickupAppointmentTo.HasValue, () =>
        {
            RuleFor(i => i.PickupAppointmentTo!.Value)
                .GreaterThanOrEqualTo(i => i.PickupAppointmentFrom!.Value)
                .WithMessage("Pickup appointment must end after it starts.");
        });

        When(i => i.DeliveryAppointmentFrom.HasValue && i.DeliveryAppointmentTo.HasValue, () =>
        {
            RuleFor(i => i.DeliveryAppointmentTo!.Value)
                .GreaterThanOrEqualTo(i => i.DeliveryAppointmentFrom!.Value)
                .WithMessage("Delivery appointment must end after it starts.");
        });
    }
}
