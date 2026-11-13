using FluentValidation;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class UpdateAIDispatchAutonomyValidator : AbstractValidator<UpdateAIDispatchAutonomyCommand>
{
    public UpdateAIDispatchAutonomyValidator()
    {
        RuleForEach(i => i.AutoApproveTypes)
            .Must(AIDispatchPolicy.AutoApproveEligibleTypes.Contains)
            .WithMessage("This decision type can never be auto-approved.");
        RuleFor(i => i.AutoApproveStreak).InclusiveBetween(1, 500);
        RuleFor(i => i.MinutesSavedPerDecision).InclusiveBetween(0, 240);
    }
}
