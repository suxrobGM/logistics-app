using FluentValidation;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty();
    }
}