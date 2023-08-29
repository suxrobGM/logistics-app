using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal class GetUsersValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(1);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}