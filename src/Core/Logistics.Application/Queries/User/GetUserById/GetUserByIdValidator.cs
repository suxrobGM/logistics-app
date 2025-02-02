using FluentValidation;

namespace Logistics.Application.Queries;

internal sealed class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator()
    {
        RuleFor(i => i.UserId)
            .NotEmpty();
    }
}