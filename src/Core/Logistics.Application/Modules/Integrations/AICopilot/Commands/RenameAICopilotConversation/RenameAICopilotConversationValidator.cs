using FluentValidation;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class RenameAICopilotConversationValidator
    : AbstractValidator<RenameAICopilotConversationCommand>
{
    public RenameAICopilotConversationValidator()
    {
        // 120 matches the auto-derived title cap in AgentTurnService.DeriveTitle.
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
    }
}
