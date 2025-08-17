using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RemoveLoadFromTripValidator : AbstractValidator<RemoveLoadFromTripCommand>
{
    public RemoveLoadFromTripValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
        RuleFor(i => i.LoadId).NotEmpty();
    }
}
