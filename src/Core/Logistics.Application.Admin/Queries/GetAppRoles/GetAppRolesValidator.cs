using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetAppRolesValidator : AbstractValidator<GetAppRolesQuery>
{
    public GetAppRolesValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
