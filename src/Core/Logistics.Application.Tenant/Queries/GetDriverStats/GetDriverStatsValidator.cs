using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal class GetDriverStatsValidator : AbstractValidator<GetDriverStatsQuery>
{
    public GetDriverStatsValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
