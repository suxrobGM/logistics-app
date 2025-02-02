using FluentValidation;

namespace Logistics.Application.Queries;

internal class GetDriverStatsValidator : AbstractValidator<GetDriverStatsQuery>
{
    public GetDriverStatsValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
