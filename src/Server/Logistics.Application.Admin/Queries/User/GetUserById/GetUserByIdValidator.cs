using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator()
    {
        RuleFor(i => i.UserId)
            .NotEmpty();
    }
}