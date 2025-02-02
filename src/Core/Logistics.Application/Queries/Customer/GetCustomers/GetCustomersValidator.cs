using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetCustomersValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
