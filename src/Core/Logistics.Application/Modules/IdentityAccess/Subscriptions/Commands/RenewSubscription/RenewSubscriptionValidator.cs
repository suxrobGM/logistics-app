using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

internal sealed class RenewSubscriptionValidator : AbstractValidator<RenewSubscriptionCommand>
{
    public RenewSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
