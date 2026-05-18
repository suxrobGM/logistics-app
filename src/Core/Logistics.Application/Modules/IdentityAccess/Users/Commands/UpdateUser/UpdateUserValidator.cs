using FluentValidation;

namespace Logistics.Application.Modules.IdentityAccess.Users.Commands;

internal sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(i => i.Id).NotEmpty();
    }
}
