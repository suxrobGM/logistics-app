using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckDriversValidator : AbstractValidator<GetTruckDriversQuery>
{
    public GetTruckDriversValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
