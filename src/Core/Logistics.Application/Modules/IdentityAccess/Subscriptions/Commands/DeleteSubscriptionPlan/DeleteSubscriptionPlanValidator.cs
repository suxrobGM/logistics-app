using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

internal sealed class DeleteSubscriptionPlanValidator : AbstractValidator<DeleteSubscriptionPlanCommand>
{
    public DeleteSubscriptionPlanValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
