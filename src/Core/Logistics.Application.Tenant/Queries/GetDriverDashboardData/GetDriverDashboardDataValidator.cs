using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDriverDashboardDataValidator : AbstractValidator<GetDriverDashboardDataQuery>
{
    public GetDriverDashboardDataValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
