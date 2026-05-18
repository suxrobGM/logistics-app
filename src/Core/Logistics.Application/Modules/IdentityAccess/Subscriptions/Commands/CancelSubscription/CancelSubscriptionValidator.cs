using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

internal sealed class CancelSubscriptionValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
