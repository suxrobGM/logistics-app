using FluentValidation;

namespace Logistics.Application.Modules.Tenancy.Subscriptions.Queries;

internal sealed class GetSubscriptionsValidator : AbstractValidator<GetSubscriptionsQuery>
{
    public GetSubscriptionsValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);

        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
