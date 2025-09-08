using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class OptimizeTripStopsValidator : AbstractValidator<OptimizeTripStopsCommand>
{
    public OptimizeTripStopsValidator()
    {
        // RuleFor(i => i.Strategy).NotEmpty().IsInEnum();

        RuleFor(i => i.Stops).NotEmpty().Must(i => i.Count() >= 2)
            .WithMessage("At least two stops are required to optimize routes.");

        RuleFor(i => i.MaxVehicles).NotEmpty().GreaterThan(0);
    }
}
