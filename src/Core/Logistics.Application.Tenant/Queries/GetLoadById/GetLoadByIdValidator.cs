using FluentValidation;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadByIdIdValidator : AbstractValidator<GetLoadByIdQuery>
{
    public GetLoadByIdIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
