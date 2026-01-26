using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class MarkStopArrivedValidator : AbstractValidator<MarkStopArrivedCommand>
{
    public MarkStopArrivedValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
        RuleFor(i => i.StopId).NotEmpty();
    }
}
