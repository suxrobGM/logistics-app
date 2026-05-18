using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

internal sealed class GetSubscriptionValidator : AbstractValidator<GetSubscriptionQuery>
{
    public GetSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
