using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
