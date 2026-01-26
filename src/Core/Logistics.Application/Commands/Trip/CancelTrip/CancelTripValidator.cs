using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class CancelTripValidator : AbstractValidator<CancelTripCommand>
{
    public CancelTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
        // Reason is optional
    }
}
