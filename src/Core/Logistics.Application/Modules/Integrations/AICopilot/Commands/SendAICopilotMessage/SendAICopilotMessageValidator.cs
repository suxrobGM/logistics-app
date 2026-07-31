using FluentValidation;

namespace Logistics.Application.Modules.Integrations.AICopilot.Commands;

internal sealed class SendAICopilotMessageValidator : AbstractValidator<SendAICopilotMessageCommand>
{
    public SendAICopilotMessageValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.PageContext).MaximumLength(300);
    }
}
