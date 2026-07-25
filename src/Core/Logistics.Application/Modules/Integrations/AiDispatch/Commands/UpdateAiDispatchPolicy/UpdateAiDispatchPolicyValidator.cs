using FluentValidation;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

internal sealed class UpdateAiDispatchPolicyValidator : AbstractValidator<UpdateAiDispatchPolicyCommand>
{
    public UpdateAiDispatchPolicyValidator()
    {
        // The prompt injection cap: anything longer would be silently clamped before the agent ever
        // saw it, so reject it here instead of pretending it was saved in full.
        RuleFor(i => i.ManualContent)
            .MaximumLength(DispatchPolicyText.MaxContentChars)
            .WithMessage($"Directives cannot exceed {DispatchPolicyText.MaxContentChars} characters.");
    }
}
