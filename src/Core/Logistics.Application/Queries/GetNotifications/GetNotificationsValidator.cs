using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsValidator()
    {
        RuleFor(i => i.StartDate).LessThan(i => i.EndDate);
    }
}
