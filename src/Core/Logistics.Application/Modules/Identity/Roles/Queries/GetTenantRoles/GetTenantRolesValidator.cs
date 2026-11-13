using FluentValidation;

namespace Logistics.Application.Modules.Identity.Roles.Queries;

internal sealed class GetTenantRolesValidator : AbstractValidator<GetTenantRolesQuery>
{
    public GetTenantRolesValidator()
    {
        RuleFor(i => i.Page)
            .GreaterThanOrEqualTo(0);

        RuleFor(i => i.PageSize)
            .GreaterThanOrEqualTo(1);
    }
}
