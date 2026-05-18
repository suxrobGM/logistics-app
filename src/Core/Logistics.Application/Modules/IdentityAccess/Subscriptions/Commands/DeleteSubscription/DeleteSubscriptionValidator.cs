using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

internal sealed class DeleteSubscriptionValidator : AbstractValidator<DeleteSubscriptionCommand>
{
    public DeleteSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
