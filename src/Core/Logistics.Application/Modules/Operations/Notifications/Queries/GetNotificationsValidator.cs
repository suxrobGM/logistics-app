using FluentValidation;

namespace Logistics.Application.Modules.Operations.Notifications.Queries;

internal sealed class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsValidator()
    {
        RuleFor(i => i.FromDate).LessThan(i => i.ToDate);
    }
}
