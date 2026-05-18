using FluentValidation;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

internal sealed class GetContainersValidator : AbstractValidator<GetContainersQuery>
{
    public GetContainersValidator()
    {
        RuleFor(i => i.Page).GreaterThan(0);
        RuleFor(i => i.PageSize).GreaterThan(0);
    }
}
