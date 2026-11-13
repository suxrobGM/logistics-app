using FluentValidation;

namespace Logistics.Application.Modules.Identity.Admins.Commands;

internal sealed class AddAdminValidator : AbstractValidator<AddAdminCommand>
{
    public AddAdminValidator()
    {
        RuleFor(i => i.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(i => i.PersonalMessage)
            .MaximumLength(500);
    }
}
