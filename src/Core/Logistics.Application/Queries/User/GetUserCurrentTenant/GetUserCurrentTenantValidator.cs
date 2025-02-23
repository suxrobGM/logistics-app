using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetUserCurrentTenantValidator : AbstractValidator<GetUserCurrentTenantQuery>
{
    public GetUserCurrentTenantValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
    }
}
