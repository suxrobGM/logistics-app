using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class UpdateTripValidator : AbstractValidator<UpdateTripCommand>
{
    public UpdateTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
