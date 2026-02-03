using FluentValidation;

namespace Logistics.Application.Commands;

public class ImpersonateUserValidator : AbstractValidator<ImpersonateUserCommand>
{
    public ImpersonateUserValidator()
    {
        RuleFor(x => x.TargetEmail)
            .NotEmpty().WithMessage("Target email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.MasterPassword)
            .NotEmpty().WithMessage("Master password is required.");
    }
}
