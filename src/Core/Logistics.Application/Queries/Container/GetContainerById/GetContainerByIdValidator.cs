using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetContainerByIdValidator : AbstractValidator<GetContainerByIdQuery>
{
    public GetContainerByIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
