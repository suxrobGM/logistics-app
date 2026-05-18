using FluentValidation;

namespace Logistics.Application.Modules.Operations.Containers.Queries;

internal sealed class GetContainerByIdValidator : AbstractValidator<GetContainerByIdQuery>
{
    public GetContainerByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
