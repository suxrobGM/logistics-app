using FluentValidation;

namespace Logistics.Application.Modules.Identity.Roles.Queries;

internal sealed class GetRolePermissionsValidator : AbstractValidator<GetRolePermissionsQuery>
{
    public GetRolePermissionsValidator()
    {
        RuleFor(i => i.RoleName)
            .NotEmpty();
    }
}
