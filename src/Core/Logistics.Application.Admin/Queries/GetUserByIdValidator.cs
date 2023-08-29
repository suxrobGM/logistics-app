using FluentValidation;

namespace Logistics.Application.Admin.Queries;

internal class GetUserByIdValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdValidator()
    {
        RuleFor(i => i.UserId)
            .NotEmpty();
    }
}