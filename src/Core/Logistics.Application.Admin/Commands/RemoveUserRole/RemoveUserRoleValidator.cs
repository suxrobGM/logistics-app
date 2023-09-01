using FluentValidation;

namespace Logistics.Application.Admin.Commands;

internal sealed class RemoveUserRoleValidator : AbstractValidator<RemoveUserRoleCommand>
{
    public RemoveUserRoleValidator()
    {
        RuleFor(i => i.UserId)
            .NotEmpty();
        
        RuleFor(i => i.Role)
            .NotEmpty();
    }
}