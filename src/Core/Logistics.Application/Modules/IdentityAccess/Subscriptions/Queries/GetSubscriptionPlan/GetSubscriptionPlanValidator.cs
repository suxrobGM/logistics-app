using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Queries;

internal sealed class GetSubscriptionPlanValidator : AbstractValidator<GetSubscriptionPlanQuery>
{
    public GetSubscriptionPlanValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
