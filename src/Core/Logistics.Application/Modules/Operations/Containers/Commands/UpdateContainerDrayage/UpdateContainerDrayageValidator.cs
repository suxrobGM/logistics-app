using FluentValidation;

namespace Logistics.Application.Modules.Operations.Containers.Commands;

internal sealed class UpdateContainerDrayageValidator : AbstractValidator<UpdateContainerDrayageCommand>
{
    public UpdateContainerDrayageValidator()
    {
        RuleFor(i => i.SteamshipLineScac)
            .Matches("^[A-Za-z]{2,4}$")
            .When(i => !string.IsNullOrEmpty(i.SteamshipLineScac))
            .WithMessage("SCAC must be 2-4 letters.");
        RuleFor(i => i.SteamshipLineName).MaximumLength(100);
        RuleFor(i => i.PickupAppointmentNumber).MaximumLength(50);
        RuleFor(i => i.ReturnAppointmentNumber).MaximumLength(50);
        RuleFor(i => i.ChassisNumber).MaximumLength(20);

        RuleFor(i => i.DemurrageRatePerDay).GreaterThanOrEqualTo(0);
        RuleFor(i => i.PerDiemRatePerDay).GreaterThanOrEqualTo(0);
        RuleFor(i => i.ChassisRatePerDay).GreaterThanOrEqualTo(0);

        RuleFor(i => i.PickupAppointmentTo)
            .GreaterThanOrEqualTo(i => i.PickupAppointmentFrom)
            .When(i => i.PickupAppointmentFrom.HasValue && i.PickupAppointmentTo.HasValue)
            .WithMessage("The pickup appointment window must end after it starts.");
        RuleFor(i => i.ReturnAppointmentTo)
            .GreaterThanOrEqualTo(i => i.ReturnAppointmentFrom)
            .When(i => i.ReturnAppointmentFrom.HasValue && i.ReturnAppointmentTo.HasValue)
            .WithMessage("The return appointment window must end after it starts.");
    }
}
