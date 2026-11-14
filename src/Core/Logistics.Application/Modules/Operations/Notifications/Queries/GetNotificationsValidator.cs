using FluentValidation;

namespace Logistics.Application.Modules.Operations.Notifications.Queries;

internal sealed class GetNotificationsValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsValidator()
    {
        RuleFor(i => i.FromDate).NotNull().LessThan(i => i.ToDate);
        RuleFor(i => i.ToDate).NotNull();
    }
}
