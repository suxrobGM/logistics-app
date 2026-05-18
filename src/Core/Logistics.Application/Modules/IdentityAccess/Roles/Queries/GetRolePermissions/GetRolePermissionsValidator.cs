using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Roles.Queries;

internal sealed class GetRolePermissionsValidator : AbstractValidator<GetRolePermissionsQuery>
{
    public GetRolePermissionsValidator()
    {
        RuleFor(i => i.RoleName)
            .NotEmpty();
    }
}
