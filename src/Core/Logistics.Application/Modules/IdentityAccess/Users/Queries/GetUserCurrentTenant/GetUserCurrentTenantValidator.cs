using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Users.Queries;

internal sealed class GetUserCurrentTenantValidator : AbstractValidator<GetUserCurrentTenantQuery>
{
    public GetUserCurrentTenantValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
