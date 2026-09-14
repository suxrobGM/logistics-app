using FluentValidation;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal sealed class MarkStopDepartedValidator : AbstractValidator<MarkStopDepartedCommand>
{
    public MarkStopDepartedValidator()
    {
        RuleFor(i => i.TripId).NotEmpty();
        RuleFor(i => i.StopId).NotEmpty();
    }
}
