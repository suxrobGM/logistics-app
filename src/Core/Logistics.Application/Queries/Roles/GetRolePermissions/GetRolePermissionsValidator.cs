using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetRolePermissionsValidator : AbstractValidator<GetRolePermissionsQuery>
{
    public GetRolePermissionsValidator()
    {
        RuleFor(i => i.RoleName)
            .NotEmpty();
    }
}
