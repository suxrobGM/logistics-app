using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetSubscriptionValidator : AbstractValidator<GetSubscriptionQuery>
{
    public GetSubscriptionValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
