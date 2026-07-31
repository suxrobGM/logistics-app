using FluentValidation;
using Logistics.Application.Validators;

namespace Logistics.Application.Modules.Platform.AISettings.Commands;

internal sealed class UpdateAISettingsValidator : AbstractValidator<UpdateAISettingsCommand>
{
    public UpdateAISettingsValidator()
    {
        RuleFor(i => i.Model).NotEmpty();

        RuleFor(i => i.ReasoningEffort).IsInEnum();

        RuleForEach(i => i.Plans)
            .Must(p => WeeklyAIBudgetRules.IsValid(p.WeeklyAIBudgetUsd))
            .WithMessage(WeeklyAIBudgetRules.Message);
    }
}
