using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class DispatchTripValidator : AbstractValidator<DispatchTripCommand>
{
    public DispatchTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
