using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class RemoveRoleFromUserValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserValidator()
    {
        RuleFor(i => i.UserId).NotEmpty();
        RuleFor(i => i.Role).NotEmpty();
    }
}
