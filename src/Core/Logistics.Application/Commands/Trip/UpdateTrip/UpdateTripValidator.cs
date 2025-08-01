using FluentValidation;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

internal sealed class UpdateTripValidator : AbstractValidator<UpdateTripCommand>
{
    public UpdateTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
    }
}
