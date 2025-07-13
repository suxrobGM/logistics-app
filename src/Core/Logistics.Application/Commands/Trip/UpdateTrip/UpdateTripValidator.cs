using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class UpdateTripValidator : AbstractValidator<UpdateTripCommand>
{
    public UpdateTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
