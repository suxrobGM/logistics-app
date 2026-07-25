using FluentValidation;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class UpdateAIDispatchPolicyValidator : AbstractValidator<UpdateAIDispatchPolicyCommand>
{
    public UpdateAIDispatchPolicyValidator()
    {
        // The prompt injection cap - anything longer gets silently clamped, so reject it instead.
        RuleFor(i => i.ManualContent)
            .MaximumLength(DispatchPolicyText.MaxContentChars)
            .WithMessage($"Directives cannot exceed {DispatchPolicyText.MaxContentChars} characters.");
    }
}
