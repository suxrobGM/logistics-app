using FluentValidation;
using Logistics.Shared.Consts;

namespace Logistics.Application.Commands;

internal sealed class CreateTripValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripValidator()
    {
        RuleFor(i => i.Name).NotEmpty();
        RuleFor(i => i.TruckId).NotEmpty();
        
        RuleFor(i => i.PlannedStart)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Planned start time must be in the future.");
    }
}
