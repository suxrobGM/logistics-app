using FluentValidation;

namespace Logistics.Application.Modules.AI.AISettings.Commands;

internal sealed class UpdateAISettingsValidator : AbstractValidator<UpdateAISettingsCommand>
{
    public UpdateAISettingsValidator()
    {
        RuleFor(i => i.Model).NotEmpty();

        RuleFor(i => i.ReasoningEffort).IsInEnum();

        RuleForEach(i => i.Plans)
            .Must(p => p.WeeklyAIBudgetUsd > 0)
            .WithMessage(WeeklyAIBudgetRules.Message);
    }
}
