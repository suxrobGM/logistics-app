using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetSubscriptionPlanValidator : AbstractValidator<GetSubscriptionPlanQuery>
{
    public GetSubscriptionPlanValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
