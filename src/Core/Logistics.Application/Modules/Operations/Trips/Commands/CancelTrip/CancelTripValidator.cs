using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class CancelTripValidator : AbstractValidator<CancelTripCommand>
{
    public CancelTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
        // Reason is optional
    }
}
