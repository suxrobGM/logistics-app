using FluentValidation;
using Logistics.Application.Modules.Operations.Loads.Commands;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class UpdateTripValidator : AbstractValidator<UpdateTripCommand>
{
    public UpdateTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
        RuleForEach(i => i.NewLoads).SetValidator(new LoadChargesValidator());
    }
}
