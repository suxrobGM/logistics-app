using FluentValidation;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

internal sealed class CreateInvitationValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationValidator()
    {
        RuleFor(i => i.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(i => i.TenantRole)
            .NotEmpty();

        RuleFor(i => i.ExpirationDays)
            .InclusiveBetween(1, 30);

        RuleFor(i => i.PersonalMessage)
            .MaximumLength(500);

        When(i => i.Type == InvitationType.CustomerUser,
            () => RuleFor(i => i.CustomerId).NotEmpty()
                .WithMessage("CustomerId is required for customer user invitations."));
    }
}
