using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateTripValidator : AbstractValidator<UpdateTripCommand>
{
    public UpdateTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();

        When(i => i.PlannedStart is not null, () =>
            RuleFor(i => i.PlannedStart)
                .Must(dt => dt > DateTime.UtcNow)
                .WithMessage("Planned start time must be in the future."));
    }
}
