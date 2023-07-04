using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal class GetAppRolesValidator : AbstractValidator<GetAppRolesQuery>
{
    public GetAppRolesValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(1);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}