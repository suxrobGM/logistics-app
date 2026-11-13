using FluentValidation;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

internal sealed class SendAIDispatchMessageValidator : AbstractValidator<SendAIDispatchMessageCommand>
{
    public SendAIDispatchMessageValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
    }
}
