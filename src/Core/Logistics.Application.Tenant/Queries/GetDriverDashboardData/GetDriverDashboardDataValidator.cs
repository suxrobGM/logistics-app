using FluentValidation;

namespace Logistics.Application.Tenant.Queries.GetDriverDashboardData;

internal sealed class GetDriverDashboardDataValidator : AbstractValidator<GetDriverDashboardDataQuery>
{
    public GetDriverDashboardDataValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}