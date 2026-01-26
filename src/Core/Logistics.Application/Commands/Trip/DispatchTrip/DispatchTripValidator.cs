using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class DispatchTripValidator : AbstractValidator<DispatchTripCommand>
{
    public DispatchTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
