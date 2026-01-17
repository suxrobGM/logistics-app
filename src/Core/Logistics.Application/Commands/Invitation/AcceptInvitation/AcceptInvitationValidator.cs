using FluentValidation;

namespace Logistics.Application.Commands;

internal sealed class AcceptInvitationValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationValidator()
    {
        RuleFor(i => i.Token)
            .NotEmpty();

        RuleFor(i => i.FirstName)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(i => i.LastName)
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(i => i.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(64);
    }
}
