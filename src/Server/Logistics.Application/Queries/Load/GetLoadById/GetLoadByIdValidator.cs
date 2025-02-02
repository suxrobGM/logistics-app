using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetLoadByIdIdValidator : AbstractValidator<GetLoadByIdQuery>
{
    public GetLoadByIdIdValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
